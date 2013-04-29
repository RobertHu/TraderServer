<%@ Page language="c#" Inherits="iExchange.TradingConsole.MHL.ENG.FrameMain" Codebehind="FrameMain.aspx.cs" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<HTML>
  <HEAD><TITLE>FrameMain</TITLE>
		<META http-equiv="Content-Type" content="text/html; charset=utf-8">
		<meta name="GENERATOR" Content="Microsoft Visual Studio 7.0">
		<meta name="CODE_LANGUAGE" Content="C#">
		<meta name="vs_defaultClientScript" content="JavaScript">		
		<meta name="vs_targetSchema" content="http://schemas.microsoft.com/intellisense/ie5">
  </HEAD>
	<frameset id="OuterFrameset" frameSpacing="2" rows="83,34%,52%"  tabindex="1">
		<frame id="MenuFrame" src="Menu.aspx" name="MenuFrame" noResize scrolling="no" frameborder="0"
			marginheight="-20">
			<frame id="InstrumentFrame" src="../../Instrument.aspx" name="InstrumentFrame" frameBorder="0"
				scrolling="no"  marginheight="0">			
		<frameset id="InnerFrameset" frameSpacing="2" Cols="25%,*" >
			<frameset id="frameset1" frameSpacing="2"  Cols="*,100%" >		
				<frame id="MessageFrame" src="../../Message.aspx" name="MessageFrame" frameBorder="1"
					scrolling="no"  marginheight="0">	
				<frame id="AccountFrame" src="../../Account.aspx" name="AccountFrame" frameBorder="0" scrolling="no"
					marginheight="0">
			</frameset>		
			<frameset id="OrderFrameset" frameSpacing="2" rows="50%,*" >
				<frame id="OrderFrame" src="../../Order.aspx" name="OrderFrame" frameBorder="0" scrolling="no"
					marginheight="0">
				<frame id="OpenOrderFrame" src="../../OpenOrder.aspx" name="OpenOrderFrame" frameBorder="0"
					scrolling="no" marginheight="0">
			</frameset>
		</frameset>
		<noframes>
			<p>This page requires frames, but your browser does not support them.</p>
		</noframes>
	</frameset>
</HTML>
