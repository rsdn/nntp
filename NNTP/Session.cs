using System;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.IO;
using System.Text;
using System.Collections;
using System.Threading;
using System.Reflection;
using derIgel.NNTP.Commands;

namespace derIgel
{
	namespace NNTP
	{
		// helper class
		public class Util
		{
			/// <summary>
			/// line delimeter
			/// </summary>
			public const string CRLF = "\r\n";
			/// <summary>
			/// maximum length of line
			/// </summary>
			public const int lineLength = 76;

			/// <summary>
			/// text base64 encoding
			/// </summary>
			public static string Encode(string text, bool header, Encoding encoder)
			{
				byte[] bytes = encoder.GetBytes(text);
				return header ?
					"=?" + encoder.HeaderName + "?b?" + Convert.ToBase64String(bytes) + "?="	:
					Convert.ToBase64String(bytes);
			}

			public static bool OnlyASCIISymbols(string text)
			{
				bool result = true;
				foreach (char symbol in text)
					if (symbol > 0x7f)
					{
						result = false;
						break;
					}
				return result;
			}
		}

		/// <summary>
		/// NNTP Server response
		/// </summary>
		public class Response
		{
			public class ParamsException : System.ArgumentException
			{
				public ParamsException(string error, Exception innerException)	:
					base(error, innerException) {}				
			}

			// Init NNTP Server's answer array
			static Response()
			{
				answers = new Hashtable();
				answers[100] = "100 help text follows";
				answers[200] = "200 RSDN NNTP Server -- posting allowed";
				answers[201] = "201 RSDN NNTP Server -- no posting allowed";
				answers[202] = "202 slave status noted";
				answers[205] = "205 RSDN NNTP Server closing connection - goodbye!";
				answers[211] = "211 {0} {1} {2} {3} group selected";
				answers[215] = "215 list of newsgroups follows";
				answers[220] = "220 {0} {1} article retrivied - head and body follow";
				answers[221] = "221 {0} {1} article retrivied - head follows";
				answers[222] = "222 {0} {1} article retrivied - body follows";
				answers[223] = "223 {0} {1} article retrivied - request text separately";
				answers[224] = "224 overview information follows";
				answers[230] = "230 list of new articles by message-id follows";
				answers[240] = "240 article posted ok";
				answers[340] = "340 send article to be posted. End with <CR-LF>.<CR-LF>";
				answers[231] = "231 list of new newsgroup follows";
				answers[281] = "281 authentification accepted";
				answers[381] = "381 more authentification information required";
				answers[400] = "400 service discontinued";
				answers[401] = "401 service temporarily unavailable - try later";
				answers[402] = "402 connection timeout - bye!";
				answers[411] = "411 no such news group";
				answers[412] = "412 no newsgroup has been selected";
				answers[420] = "420 no current article has been selected";
				answers[421] = "421 no next article in this group";
				answers[422] = "422 no previous article in this group";
				answers[423] = "423 no such article number in this group";
				answers[430] = "430 no such article found";
				answers[440] = "440 posting not allowed";
				answers[441] = "441 posting failed";
				answers[480] = "480 authentification required";
				answers[482] = "482 authentification rejected";
				answers[500] = "500 command not recognized";
				answers[501] = "501 command syntax error";
				answers[502] = "502 no permission";
				answers[503] = "503 program fault - command not performed";
			}

			public Response(int code, string[] parameters, string text)
			{
				this.code = code;
				this.parameters = parameters;
				bodyResponse = text;
			}
			public Response(int code, string[] parameters) : this(code, parameters, null) {}
			public Response(int code) : this(code, null, null) {}
			public Response() : this(500, null, null) {} //default error code 500 - not recognized command

			/// <summary>
			/// Associative arrays of NNTP server answers
			/// </summary>
			static protected Hashtable answers;
			/// <summary>
			/// response code
			/// </summary>
			protected int code;
			/// <summary>
			/// response parameters
			/// </summary>
			protected string[] parameters;

			public static string GetResponse(int code, string[] parameters, string bodyResponse)
			{
				string content = answers[code] as string;
				try
				{
					content += Util.CRLF;
					if (parameters != null)
						content = string.Format(content, parameters);
					if (bodyResponse != null)
						content += ModifyTextResponse(bodyResponse) + "." + Util.CRLF;
				}
				catch(FormatException e)
				{
					throw new ParamsException("Formatting error", e);
				}
				catch (ArgumentNullException e)
				{
					throw new ParamsException("No such response code", e);
				}
				return content;
			}

			public static void Answer(int code, Socket socket)
			{
				socket.Send(Encoding.ASCII.GetBytes(GetResponse(code, null, null)));
			}

			public string GetResponse()
			{
				return GetResponse(code, parameters, bodyResponse);
			}

			/// <summary>
			/// textual response (only after status response may be)
			/// </summary>
			protected string bodyResponse;

			/// <summary>
			/// Modiffy textual response as double first dot
			/// </summary>
			public static string ModifyTextResponse(string text)
			{
				text.Replace(Util.CRLF + ".", Util.CRLF+ "..");
				if ((text != string.Empty) && !text.EndsWith(Util.CRLF))
					text += Util.CRLF;
				return text;
			}

			public int Code
			{
				get
				{
					return code;
				}
			}
		}

		/// <summary>
		/// NNTP Session
		/// </summary>
		public class Session : IDisposable
		{
			static Session()
			{
				commandsTypes = new Hashtable();
				// initialize types for NNTP commands classes
				foreach (Type type in	Assembly.GetCallingAssembly().GetTypes())
					if (type.IsSubclassOf(typeof(Generic)) &&
						type.IsDefined(typeof(NNTPCommandAttribute), true) &&
						!type.IsAbstract)
					{
						foreach(object attr in type.GetCustomAttributes(typeof(NNTPCommandAttribute), true))
							commandsTypes[((NNTPCommandAttribute)attr).Name] = type;
					}
			}

			public Session(Socket client, DataProvider dataProvider, WaitHandle exitEvent)
			{
				sessionState = States.AuthRequired;

				this.exitEvent = exitEvent;
				commandBuffer = new byte[bufferSize];
				this.client = client;
				netStream = new NetworkStream(client);
				this.dataProvider = dataProvider;

				// Init client's command array
				commands = new Hashtable();
				foreach (DictionaryEntry entry in commandsTypes)
					commands[entry.Key] = 
						Activator.CreateInstance((Type)entry.Value, new Object[]{this});

			}

			protected static Hashtable commandsTypes;

			// resonse answer from server
			protected void Answer(int code)
			{
				Answer(new Response(code));
			}

			protected void Answer(Response response)
			{
				byte[] bytes = Encoding.ASCII.GetBytes(response.GetResponse());
				netStream.Write(bytes, 0, bytes.GetLength(0));
			}

			/// <summary>
			/// Process client
			/// </summary>
			public void Process(Object state)
			{
				string delimeter;
				Response result = null;
				try
				{
					// response OK
					Answer(201);

					string bufferString = string.Empty;
					while (true)
					{
						do
						{
							IAsyncResult asyncResult = netStream.BeginRead(commandBuffer, 0, bufferSize, null, null);
							switch (WaitHandle.WaitAny(
								new WaitHandle[] {asyncResult.AsyncWaitHandle, exitEvent},
								connectionTimeout, false))
							{
								case WaitHandle.WaitTimeout	:
									// timeout
									Answer(402);
									return;
								case 1	:
									// sorry, bye!
									Answer(400);
									return;
							}
							int receivedBytes = netStream.EndRead(asyncResult);
							if (receivedBytes  > 0)
								bufferString += Encoding.ASCII.GetString(commandBuffer, 0, receivedBytes);
							else
								return;
						}
						while (netStream.DataAvailable);

						if (sessionState == States.PostWaiting)
							delimeter = Util.CRLF + "." + Util.CRLF;
						else
							delimeter = Util.CRLF;
						if (bufferString.IndexOf(delimeter) != -1)
						{
							// get command string till delimeter
							commandString = bufferString.Substring(0, bufferString.IndexOf(delimeter));
							
//							Console.Error.WriteLine(commandString);

							bufferString = bufferString.Remove(0,
								bufferString.IndexOf(delimeter) + delimeter.Length);
						
							if ((bufferString != string.Empty) && (bufferString.Trim() == string.Empty))
								bufferString = string.Empty;

							try
							{
								if (sessionState == States.PostWaiting)
								{
									dataProvider.PostMessage(commandString);
									sessionState = States.Normal;
									result = new Response(240);
								}
								else
								{
									// get first word in upper case delimeted by space or tab characters 
									command = commandString.Split(new char[]{' ', '\t', '\r'}, 2)[0].ToUpper();
									// check suppoting command
									if (commands[command] != null)
									{	
										Commands.Generic nntpCommand = commands[command] as Commands.Generic;
										switch (sessionState)
										{
											case	States.AuthRequired	:
											case	States.MoreAuthRequired	:
												if ((command == "AUTHINFO") || (command == "QUIT"))
													goto default;
												else
												{
													if (sessionState == States.AuthRequired)
														result = new Response(480);
													else
														result = new Response(381);
													break;
												}
											default	:
												result = nntpCommand.Process();
												break;
										}
									}
									else
										// no such command
										result = new Response(500);
								}

								Answer(result);
//								Console.Error.Write(result.GetResponse());

								if (result.Code == 205) //quit response
									return;
							}
							catch (System.NullReferenceException)
							{
								Answer(503);
							}
							catch (Response.ParamsException)
							{
								Answer(503);
							}
							catch (DataProvider.Exception exception)
							{
								Response response;
								switch (exception.Error)
								{
									case DataProvider.Errors.NoSuchGroup:
										response = new Response(411);
										break;
									case DataProvider.Errors.NoSelectedGroup:
										response = new Response(412);
										break;
									case DataProvider.Errors.NoSelectedArticle:
										response = new Response(420);
										break;
									case DataProvider.Errors.NoNextArticle:
										response = new Response(421);
										break;
									case DataProvider.Errors.NoPrevArticle:
										response = new Response(422);
										break;
									case DataProvider.Errors.NoSuchArticleNumber:
										response = new Response(423);
										break;
									case DataProvider.Errors.NoSuchArticle:
										response = new Response(430);
										break;
									case DataProvider.Errors.NoPermission:
										response = new Response(502);
										sessionState = States.AuthRequired;
										break;
									case DataProvider.Errors.NotSupported:
										response = new Response(500);
										break;
									case DataProvider.Errors.PostingFailed:
										response = new Response(441);
										break;
									default:
										response = new Response(503); //error
										break;
								}
								Answer(response);
							}
							catch(Exception)
							{
								Answer(503);
							}
						}
					}
				}
				catch (IOException)
				{
					// network error
				}
				finally
				{
					Close();
				}
			}

			/// <summary>
			/// network client
			/// </summary>
			protected Socket client;
			/// <summary>
			/// network stream for client's session
			/// </summary>
			protected NetworkStream netStream;
			/// <summary>
			/// size of commandBuffer
			/// </summary>
			protected const int bufferSize = 512;
			/// <summary>
			/// buffer for command string bytes
			/// </summary>
			protected byte[] commandBuffer;
			/// <summary>
			/// last received command string
			/// </summary>
			protected internal string commandString;
			/// <summary>
			/// last received command
			/// </summary>
			string command;

			/// <summary>
			/// NNTP Session states
			/// </summary>
			public enum States {Normal, AuthRequired, MoreAuthRequired, PostWaiting}
		
			/// <summary>
			/// State of current session
			/// </summary>
			protected internal States sessionState = States.Normal;
			/// <summary>
			/// Associative arrays of NNTP client commands
			/// </summary>
			protected internal Hashtable commands;
			/// <summary>
			/// Current selected group
			/// </summary>
			protected string currentGroup = null;
			/// <summary>
			/// Current selected article
			/// </summary>
			protected int currentArticle = -1;

			/// <summary>
			/// close connection
			/// </summary>
			public void Close()
			{
				Dispose();
			}

			protected internal DataProvider dataProvider;
			/// <summary>
			/// connection timeout interval in milliseconds (3 min)
			/// </summary>
			protected const int connectionTimeout = 180000;

			public void Dispose()
			{
				netStream.Close();
				client.Shutdown(SocketShutdown.Both);
				client.Close();		
				if (Disposed != null)
					Disposed(this, EventArgs.Empty);
			}

			public event EventHandler Disposed;

			protected WaitHandle exitEvent;
		}
	}
}