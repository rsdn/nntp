<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0//EN"
     "http://www.w3.org/TR/REC-html40/strict.dtd">
<html>
	<head>
		<meta http-equiv="Content-Type" content="text/html; charset=$encoding.WebName$">
		<base href="$server$/forum/">
		<link href="Forum.css" type="text/css" rel="stylesheet">
	</head>
	<body style="margin: 0px;">
		<table cellSpacing="0" width="100%" border="0">
			<tr>
				<td class="s" noWrap style="font-weight: normal;">
					From:&nbsp;<A$if(message.AuthorID)$ href="/Users/Profile.aspx?uid=$message.AuthorID$$endif$" target="_blank"><B>$if(message.Author)$$message.Author$$else$Аноним$endif$</B></A>$if(message.UserType)$&nbsp;<SMALL><span style="color: $message.UserColorWeb$; font-size: smaller;">$message.UserType$</span>$endif$</SMALL>
				</td>
				<td style="display: none">$replyMarker$</td>
				<td class="s" noWrap align="center" width="100%">$homepage$</td>
				<td class="s" noWrap>
					<A title="Поиск в форуме" href="/search/?group=$message.GroupID$" target="_blank"><IMG height="14" src="/images/search.gif" width="18" align="absMiddle" border="0" ></A>
					<A title="Добавить в избранное" href="/Users/Private/AddFav.aspx?mid=$message.ID$" target="_blank"><IMG height="14" src="/images/fav.gif" align="absMiddle" border="0"></A>
					<A title="Написать новое сообщение" href="NewMsg.aspx?gid=$message.GroupID$" target="_blank"><IMG height="14" src="images/new.gif" width="18" align="absMiddle" border="0"></A>
					<A title="Ответить на сообщение" href="NewMsg.aspx?mid=$message.ID$" target="_blank"><IMG height="14" src="images/replay.gif" width="18" align="absMiddle" border="0"></A>
					<A title="Перейти в форум" href="?mid=$message.ID$" target="_blank"><IMG height="14" src="images/thr.gif" width="18" align="absMiddle" border="0"></A><A title="Просмотреть все ответы темы" href="Message.aspx?mid=$message.ID$#$message.ID$"><IMG height="14" src="images/flat.gif" width="18" align="absMiddle" border="0"></A>
					<A title="Редактировать сообщение" href="NewMsg.aspx?mid=$message.ID$&amp;edit=1" target="_blank"><IMG height="14" src="images/edit.gif" width="18" align="absMiddle" border="0"></A>
					<A title="Модерирование" href="Private/Self.aspx?mid=$message.ID$"><IMG height="14" src="images/del.gif" width="18" align="absMiddle" border="0"></A>
				</td>
			</tr>
		</table>
		<div class="m">$text$</div>
		<div class="o"><font size="2">$origin$</font></div>
		<table cellSpacing="0" width="100%" border="0">
			<tr>
				<td class="s" style="font-weight: normal;">
					<a title="Сообщение на RSDN.RU" href="?mid=$message.ID$" target="_blank">$subject$</a>
				</td>
				<td class="s" align="right" style="font-weight: normal;"><A href="RateList.aspx?mid=$message.ID$">Оценить</A>
					<A title="+1" href="Private/Rate.aspx?mid=$message.ID$&amp;rate=-3" target="_blank"><IMG height="14" src="images/n11.gif" width="18" align="absMiddle" border="0"></A><A title="Интересно" href="Private/Rate.aspx?mid=$message.ID$&amp;rate=1" target="_blank"><IMG height="14" src="images/n1.gif" width="18" align="absMiddle" border="0"></A><A title="Спасибо" href="Private/Rate.aspx?mid=$message.ID$&amp;rate=2" target="_blank"><IMG height="14" src="images/n2.gif" width="18" align="absMiddle" border="0"></A><A title="Супер" href="Private/Rate.aspx?mid=$message.ID$&amp;rate=3" target="_blank"><IMG height="14" src="images/n3.gif" width="18" align="absMiddle" border="0"></A><A title="Удалить оценку" href="Private/Rate.aspx?mid=$message.ID$&amp;rate=-1" target="_blank"><IMG id="IMG1" height="14" src="images/nx.gif" width="18" align="absMiddle" border="0"></A>
					<A title="Смешно" href="Private/Rate.aspx?mid=$message.ID$&amp;rate=-2" target="_blank"><IMG src="images/smile.gif" align="absMiddle" border="0"></A>
					<A title="Согласен" href="Private/Rate.aspx?mid=$message.ID$&amp;rate=-4" target="_blank"><IMG height="14" src="images/np.gif" width="18" align="absMiddle" border="0"></A><A title="Не согласен" href="Private/Rate.aspx?mid=$message.ID$&amp;rate=0" target="_blank"><IMG height="14" src="images/nm.gif" width="18" align="absMiddle" border="0"></A>
				</td>
				<td style="display: none">$replyMarker$</td>
			</tr>
		</table>
	</body>
</html>
