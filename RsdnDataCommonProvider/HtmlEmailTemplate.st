<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0//EN"
     "http://www.w3.org/TR/REC-html40/strict.dtd">
<html>
	<head>
		<title>$subject$</title>
		<meta http-equiv="Content-Type" content="text/html; charset=$encoding.WebName$" />
		<base href="$server$/forum/" />
		<link href="Forum.css" type="text/css" rel="stylesheet" />
	</head>
	<body style="margin: 0px;">
		<table cellspacing="0" width="100%" border="0">
			<tr>
				<td class="s" nowrap="nowrap" style="font-weight: normal;">
					From:&nbsp;<a $if(message.AuthorID)$href="/Users/$message.AuthorID$.aspx$endif$" target="_blank"><b>$if(message.Author)$$message.Author$$else$Аноним$endif$</b></a>$if(message.UserType)$&nbsp;<small><span style="color: $message.UserColorWeb$; font-size: smaller;">$message.UserType$</span>$endif$</small>
				</td>
				<td style="display: none">$replyMarker$</td>
				<td class="s" nowrap="nowrap" align="center" width="100%">$homepage$</td>
				<td class="s" nowrap="nowrap">
					<a title="Поиск в форуме" href="/search/?group=$message.GroupID$" target="_blank"><img height="14" src="/images/search.gif" width="18" align="absmiddle" border="0" alt="search" /></a>
					<a title="Добавить в избранное" href="/Users/Private/addFav.aspx?mid=$message.ID$" target="_blank"><img height="14" src="/images/fav.gif" align="absmiddle" border="0" alt="fav" /></a>
					<a title="Написать новое сообщение" href="NewMsg.aspx?gid=$message.GroupID$" target="_blank"><img height="14" src="images/new.gif" width="18" align="absmiddle" border="0" alt="new" /></a>
					<a title="Ответить на сообщение" href="NewMsg.aspx?mid=$message.ID$" target="_blank"><img height="14" src="images/replay.gif" width="18" align="absmiddle" border="0" alt="replay" /></a>
					<a title="Перейти в форум" href="message/$message.ID$.aspx" target="_blank"><img height="14" src="images/thr.gif" width="18" align="absmiddle" border="0" alt="thread" /></a><a title="Просмотреть все ответы темы" href="message/$message.ID$.flat.aspx#$message.ID$"><img height="14" src="images/flat.gif" width="18" align="absmiddle" border="0" alt="flat" /></a>
					<a title="Редактировать сообщение" href="NewMsg.aspx?mid=$message.ID$&amp;edit=1" target="_blank"><img height="14" src="images/edit.gif" width="18" align="absmiddle" border="0" alt="edit" /></a>
					<a title="Модерирование" href="Private/Self.aspx?mid=$message.ID$"><img height="14" src="images/del.gif" width="18" align="absmiddle" border="0" alt="del" /></a>
				</td>
			</tr>
		</table>
		<div class="m">$text$</div>
		<div class="o"><font size="2">$origin$</font></div>
		<table cellspacing="0" width="100%" border="0">
			<tr>
				<td class="s" style="font-weight: normal;">
					<a title="Сообщение на RSDN.RU" href="message/$message.ID$.aspx" target="_blank">$subject$</a>
				</td>
				<td class="s" align="right" style="font-weight: normal;"><a href="RateList.aspx?mid=$message.ID$">Оценить</a>
					<a title="+1" href="Private/Rate.aspx?mid=$message.ID$&amp;rate=-3" target="_blank"><img height="14" src="images/n11.gif" width="18" align="absmiddle" border="0" alt="+1" /></a><a title="Интересно" href="Private/Rate.aspx?mid=$message.ID$&amp;rate=1" target="_blank"><img height="14" src="images/n1.gif" width="18" align="absmiddle" border="0" alt="1" /></a><a title="Спасибо" href="Private/Rate.aspx?mid=$message.ID$&amp;rate=2" target="_blank"><img height="14" src="images/n2.gif" width="18" align="absmiddle" border="0" alt="2" /></a><a title="Супер" href="Private/Rate.aspx?mid=$message.ID$&amp;rate=3" target="_blank"><img height="14" src="images/n3.gif" width="18" align="absmiddle" border="0" alt="3" /></a><a title="Удалить оценку" href="Private/Rate.aspx?mid=$message.ID$&amp;rate=-1" target="_blank"><img id="IMG1" height="14" src="images/nx.gif" width="18" align="absmiddle" border="0" alt="X" /></a>
					<a title="Смешно" href="Private/Rate.aspx?mid=$message.ID$&amp;rate=-2" target="_blank"><img src="images/smile.gif" align="absmiddle" border="0" alt=":)" /></a>
					<a title="Согласен" href="Private/Rate.aspx?mid=$message.ID$&amp;rate=-4" target="_blank"><img height="14" src="images/np.gif" width="18" align="absmiddle" border="0" alt="+" /></a><a title="Не согласен" href="Private/Rate.aspx?mid=$message.ID$&amp;rate=0" target="_blank"><img height="14" src="images/nm.gif" width="18" align="absmiddle" border="0" alt="-" /></a>
				</td>
				<td style="display: none">$replyMarker$</td>
			</tr>
		</table>
	</body>
</html>
