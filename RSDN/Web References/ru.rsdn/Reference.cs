﻿//------------------------------------------------------------------------------
// <autogenerated>
//     This code was generated by a tool.
//     Runtime Version: 1.0.3705.288
//
//     Changes to this file may cause incorrect behavior and will be lost if 
//     the code is regenerated.
// </autogenerated>
//------------------------------------------------------------------------------

// 
// This source code was auto-generated by Microsoft.VSDesigner, Version 1.0.3705.288.
// 
namespace derIgel.RsdnNntp.ru.rsdn {
    using System.Diagnostics;
    using System.Xml.Serialization;
    using System;
    using System.Web.Services.Protocols;
    using System.ComponentModel;
    using System.Web.Services;
    
    
    /// <remarks/>
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Web.Services.WebServiceBindingAttribute(Name="ForumSoap", Namespace="http://rsdn.ru/ws/")]
    public class Forum : System.Web.Services.Protocols.SoapHttpClientProtocol {
        
        /// <remarks/>
        public Forum() {
            this.Url = "http://rsdn.ru/ws/forum.asmx";
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://rsdn.ru/ws/GroupList", RequestNamespace="http://rsdn.ru/ws/", ResponseNamespace="http://rsdn.ru/ws/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public group_list GroupList(string login, string psw) {
            object[] results = this.Invoke("GroupList", new object[] {
                        login,
                        psw});
            return ((group_list)(results[0]));
        }
        
        /// <remarks/>
        public System.IAsyncResult BeginGroupList(string login, string psw, System.AsyncCallback callback, object asyncState) {
            return this.BeginInvoke("GroupList", new object[] {
                        login,
                        psw}, callback, asyncState);
        }
        
        /// <remarks/>
        public group_list EndGroupList(System.IAsyncResult asyncResult) {
            object[] results = this.EndInvoke(asyncResult);
            return ((group_list)(results[0]));
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://rsdn.ru/ws/GetGroupList", RequestNamespace="http://rsdn.ru/ws/", ResponseNamespace="http://rsdn.ru/ws/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public group_list GetGroupList(string login, string psw, System.DateTime dtc) {
            object[] results = this.Invoke("GetGroupList", new object[] {
                        login,
                        psw,
                        dtc});
            return ((group_list)(results[0]));
        }
        
        /// <remarks/>
        public System.IAsyncResult BeginGetGroupList(string login, string psw, System.DateTime dtc, System.AsyncCallback callback, object asyncState) {
            return this.BeginInvoke("GetGroupList", new object[] {
                        login,
                        psw,
                        dtc}, callback, asyncState);
        }
        
        /// <remarks/>
        public group_list EndGetGroupList(System.IAsyncResult asyncResult) {
            object[] results = this.EndInvoke(asyncResult);
            return ((group_list)(results[0]));
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://rsdn.ru/ws/GroupInfo", RequestNamespace="http://rsdn.ru/ws/", ResponseNamespace="http://rsdn.ru/ws/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public group GroupInfo(string name, string login, string psw) {
            object[] results = this.Invoke("GroupInfo", new object[] {
                        name,
                        login,
                        psw});
            return ((group)(results[0]));
        }
        
        /// <remarks/>
        public System.IAsyncResult BeginGroupInfo(string name, string login, string psw, System.AsyncCallback callback, object asyncState) {
            return this.BeginInvoke("GroupInfo", new object[] {
                        name,
                        login,
                        psw}, callback, asyncState);
        }
        
        /// <remarks/>
        public group EndGroupInfo(System.IAsyncResult asyncResult) {
            object[] results = this.EndInvoke(asyncResult);
            return ((group)(results[0]));
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://rsdn.ru/ws/GetArticle", RequestNamespace="http://rsdn.ru/ws/", ResponseNamespace="http://rsdn.ru/ws/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public article GetArticle(string group, int number, string login, string psw) {
            object[] results = this.Invoke("GetArticle", new object[] {
                        group,
                        number,
                        login,
                        psw});
            return ((article)(results[0]));
        }
        
        /// <remarks/>
        public System.IAsyncResult BeginGetArticle(string group, int number, string login, string psw, System.AsyncCallback callback, object asyncState) {
            return this.BeginInvoke("GetArticle", new object[] {
                        group,
                        number,
                        login,
                        psw}, callback, asyncState);
        }
        
        /// <remarks/>
        public article EndGetArticle(System.IAsyncResult asyncResult) {
            object[] results = this.EndInvoke(asyncResult);
            return ((article)(results[0]));
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://rsdn.ru/ws/GetFormattedArticle", RequestNamespace="http://rsdn.ru/ws/", ResponseNamespace="http://rsdn.ru/ws/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public article GetFormattedArticle(string group, int number, string login, string psw) {
            object[] results = this.Invoke("GetFormattedArticle", new object[] {
                        group,
                        number,
                        login,
                        psw});
            return ((article)(results[0]));
        }
        
        /// <remarks/>
        public System.IAsyncResult BeginGetFormattedArticle(string group, int number, string login, string psw, System.AsyncCallback callback, object asyncState) {
            return this.BeginInvoke("GetFormattedArticle", new object[] {
                        group,
                        number,
                        login,
                        psw}, callback, asyncState);
        }
        
        /// <remarks/>
        public article EndGetFormattedArticle(System.IAsyncResult asyncResult) {
            object[] results = this.EndInvoke(asyncResult);
            return ((article)(results[0]));
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://rsdn.ru/ws/GetArticleByID", RequestNamespace="http://rsdn.ru/ws/", ResponseNamespace="http://rsdn.ru/ws/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public article GetArticleByID(int mid, string login, string psw) {
            object[] results = this.Invoke("GetArticleByID", new object[] {
                        mid,
                        login,
                        psw});
            return ((article)(results[0]));
        }
        
        /// <remarks/>
        public System.IAsyncResult BeginGetArticleByID(int mid, string login, string psw, System.AsyncCallback callback, object asyncState) {
            return this.BeginInvoke("GetArticleByID", new object[] {
                        mid,
                        login,
                        psw}, callback, asyncState);
        }
        
        /// <remarks/>
        public article EndGetArticleByID(System.IAsyncResult asyncResult) {
            object[] results = this.EndInvoke(asyncResult);
            return ((article)(results[0]));
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://rsdn.ru/ws/GetFormattedArticleByID", RequestNamespace="http://rsdn.ru/ws/", ResponseNamespace="http://rsdn.ru/ws/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public article GetFormattedArticleByID(int mid, string login, string psw) {
            object[] results = this.Invoke("GetFormattedArticleByID", new object[] {
                        mid,
                        login,
                        psw});
            return ((article)(results[0]));
        }
        
        /// <remarks/>
        public System.IAsyncResult BeginGetFormattedArticleByID(int mid, string login, string psw, System.AsyncCallback callback, object asyncState) {
            return this.BeginInvoke("GetFormattedArticleByID", new object[] {
                        mid,
                        login,
                        psw}, callback, asyncState);
        }
        
        /// <remarks/>
        public article EndGetFormattedArticleByID(System.IAsyncResult asyncResult) {
            object[] results = this.EndInvoke(asyncResult);
            return ((article)(results[0]));
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://rsdn.ru/ws/ArticleList", RequestNamespace="http://rsdn.ru/ws/", ResponseNamespace="http://rsdn.ru/ws/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public article_list ArticleList(string group, int first, int last, string login, string psw) {
            object[] results = this.Invoke("ArticleList", new object[] {
                        group,
                        first,
                        last,
                        login,
                        psw});
            return ((article_list)(results[0]));
        }
        
        /// <remarks/>
        public System.IAsyncResult BeginArticleList(string group, int first, int last, string login, string psw, System.AsyncCallback callback, object asyncState) {
            return this.BeginInvoke("ArticleList", new object[] {
                        group,
                        first,
                        last,
                        login,
                        psw}, callback, asyncState);
        }
        
        /// <remarks/>
        public article_list EndArticleList(System.IAsyncResult asyncResult) {
            object[] results = this.EndInvoke(asyncResult);
            return ((article_list)(results[0]));
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://rsdn.ru/ws/Authentication", RequestNamespace="http://rsdn.ru/ws/", ResponseNamespace="http://rsdn.ru/ws/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public auth_info Authentication(string login, string psw) {
            object[] results = this.Invoke("Authentication", new object[] {
                        login,
                        psw});
            return ((auth_info)(results[0]));
        }
        
        /// <remarks/>
        public System.IAsyncResult BeginAuthentication(string login, string psw, System.AsyncCallback callback, object asyncState) {
            return this.BeginInvoke("Authentication", new object[] {
                        login,
                        psw}, callback, asyncState);
        }
        
        /// <remarks/>
        public auth_info EndAuthentication(System.IAsyncResult asyncResult) {
            object[] results = this.EndInvoke(asyncResult);
            return ((auth_info)(results[0]));
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://rsdn.ru/ws/GetLink", RequestNamespace="http://rsdn.ru/ws/", ResponseNamespace="http://rsdn.ru/ws/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public link GetLink(string link, LinkType type, string login, string psw) {
            object[] results = this.Invoke("GetLink", new object[] {
                        link,
                        type,
                        login,
                        psw});
            return ((link)(results[0]));
        }
        
        /// <remarks/>
        public System.IAsyncResult BeginGetLink(string link, LinkType type, string login, string psw, System.AsyncCallback callback, object asyncState) {
            return this.BeginInvoke("GetLink", new object[] {
                        link,
                        type,
                        login,
                        psw}, callback, asyncState);
        }
        
        /// <remarks/>
        public link EndGetLink(System.IAsyncResult asyncResult) {
            object[] results = this.EndInvoke(asyncResult);
            return ((link)(results[0]));
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://rsdn.ru/ws/PostMIMEMessage", RequestNamespace="http://rsdn.ru/ws/", ResponseNamespace="http://rsdn.ru/ws/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public post_result PostMIMEMessage(string login, string psw, string message) {
            object[] results = this.Invoke("PostMIMEMessage", new object[] {
                        login,
                        psw,
                        message});
            return ((post_result)(results[0]));
        }
        
        /// <remarks/>
        public System.IAsyncResult BeginPostMIMEMessage(string login, string psw, string message, System.AsyncCallback callback, object asyncState) {
            return this.BeginInvoke("PostMIMEMessage", new object[] {
                        login,
                        psw,
                        message}, callback, asyncState);
        }
        
        /// <remarks/>
        public post_result EndPostMIMEMessage(System.IAsyncResult asyncResult) {
            object[] results = this.EndInvoke(asyncResult);
            return ((post_result)(results[0]));
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://rsdn.ru/ws/PostUnicodeMessage", RequestNamespace="http://rsdn.ru/ws/", ResponseNamespace="http://rsdn.ru/ws/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public post_result PostUnicodeMessage(string login, string psw, int mid, string group, string subject, string message) {
            object[] results = this.Invoke("PostUnicodeMessage", new object[] {
                        login,
                        psw,
                        mid,
                        group,
                        subject,
                        message});
            return ((post_result)(results[0]));
        }
        
        /// <remarks/>
        public System.IAsyncResult BeginPostUnicodeMessage(string login, string psw, int mid, string group, string subject, string message, System.AsyncCallback callback, object asyncState) {
            return this.BeginInvoke("PostUnicodeMessage", new object[] {
                        login,
                        psw,
                        mid,
                        group,
                        subject,
                        message}, callback, asyncState);
        }
        
        /// <remarks/>
        public post_result EndPostUnicodeMessage(System.IAsyncResult asyncResult) {
            object[] results = this.EndInvoke(asyncResult);
            return ((post_result)(results[0]));
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://rsdn.ru/ws/")]
    public class group_list {
        
        /// <remarks/>
        public System.DateTime date;
        
        /// <remarks/>
        public string error;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute(IsNullable=false)]
        public group[] groups;
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://rsdn.ru/ws/")]
    public class group {
        
        /// <remarks/>
        public string error;
        
        /// <remarks/>
        public string name;
        
        /// <remarks/>
        public System.DateTime created;
        
        /// <remarks/>
        public int number;
        
        /// <remarks/>
        public int first;
        
        /// <remarks/>
        public int last;
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://rsdn.ru/ws/")]
    public class post_result {
        
        /// <remarks/>
        public string error;
        
        /// <remarks/>
        public bool ok;
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://rsdn.ru/ws/")]
    public class link {
        
        /// <remarks/>
        public string error;
        
        /// <remarks/>
        public string url;
        
        /// <remarks/>
        public string name;
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://rsdn.ru/ws/")]
    public class auth_info {
        
        /// <remarks/>
        public string error;
        
        /// <remarks/>
        public bool ok;
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://rsdn.ru/ws/")]
    public class article_list {
        
        /// <remarks/>
        public string error;
        
        /// <remarks/>
        public string postfix;
        
        /// <remarks/>
        public System.DateTime date;
        
        /// <remarks/>
        public article[] articles;
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://rsdn.ru/ws/")]
    public class article {
        
        /// <remarks/>
        public string error;
        
        /// <remarks/>
        public string postfix;
        
        /// <remarks/>
        public string id;
        
        /// <remarks/>
        public string pid;
        
        /// <remarks/>
        public string gid;
        
        /// <remarks/>
        public int num;
        
        /// <remarks/>
        public string author;
        
        /// <remarks/>
        public string authorid;
        
        /// <remarks/>
        public System.DateTime date;
        
        /// <remarks/>
        public string subject;
        
        /// <remarks/>
        public string message;
        
        /// <remarks/>
        public string fmtmessage;
        
        /// <remarks/>
        public string userType;
        
        /// <remarks/>
        public string homePage;
        
        /// <remarks/>
        public string group;
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://rsdn.ru/ws/")]
    public enum LinkType {
        
        /// <remarks/>
        MSDN,
    }
}
