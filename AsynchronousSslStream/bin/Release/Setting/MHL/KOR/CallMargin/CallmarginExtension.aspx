<%@ Page Language="C#" AutoEventWireup="true" Inherits="MHL_KOR_CallMargin_CallmarginExtension" Codebehind="CallmarginExtension.aspx.cs" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<HTML>
	<HEAD>
		<title>Margin Call Extension</title>
		<meta content="Microsoft Visual Studio .NET 7.1" name="GENERATOR">
		<meta content="C#" name="CODE_LANGUAGE">
		<meta content="JavaScript" name="vs_defaultClientScript">
		<meta content="http://schemas.microsoft.com/intellisense/ie5" name="vs_targetSchema">
		<script language="jscript" src="../../../Javascript/CallMarginExtensionPage.js"></script>
	</HEAD>
	<body onload="Onload();">
		<center>
			<TABLE id="Table1" cellSpacing="1" cellPadding="1" width="592" border="0" style="WIDTH: 592px; HEIGHT: 384px">
				<TR>
					<TD style="HEIGHT: 24px"></TD>
					<TD colSpan="2">
						<P align="center"><STRONG><U>EXTENSION OF MARGIN CALL</U></STRONG></P>
					</TD>
					<TD style="HEIGHT: 24px"></TD>
				</TR>
				<TR>
					<TD style="HEIGHT: 36px">&nbsp;</TD>
					<TD colSpan="2">
						<P><SPAN lang="EN-US" style="FONT-SIZE: 12pt"><SPAN lang="EN-US" style="FONT-SIZE: 12pt"> <SPAN lang="EN-US" style="FONT-SIZE: 12pt">
										<SPAN lang="EN-US" style="FONT-SIZE: 12pt"></SPAN></SPAN></SPAN></SPAN>
							<SPAN lang="EN-US" style="FONT-SIZE: 12pt; mso-bidi-font-weight: normal">
								<BR>
								<TABLE id="Table2" style="WIDTH: 568px; HEIGHT: 95px" cellSpacing="1" cellPadding="1" width="568"
									border="0">
									<TR>
										<TD style="WIDTH: 117px">Date</TD>
										<TD style="WIDTH: 28px">:</TD>
										<TD>CurrentTradeDay</TD>
									</TR>
									<TR>
										<TD style="WIDTH: 117px"><SPAN lang="EN-US" style="FONT-SIZE: 12pt">To</SPAN></TD>
										<TD style="WIDTH: 28px">:</TD>
										<TD>OrganizationName</TD>
									</TR>
									<TR>
										<TD style="WIDTH: 117px">From</TD>
										<TD style="WIDTH: 28px">:</TD>
										<TD>CustomerName, AccountCode</TD>
									</TR>
									<TR>
										<TD style="WIDTH: 117px">Registrated E-mail</TD>
										<TD style="WIDTH: 28px">:</TD>
										<TD><INPUT id="txtEmail" onblur="txtEmail_onblur();" style="FONT-SIZE: medium; WIDTH: 368px; FONT-FAMILY: System; HEIGHT: 23px; TEXT-ALIGN: left"
												accessKey="e" type="text" maxLength="80" size="56" name="txtEmail"></TD>
									</TR>
								</TABLE>
								<BR>
							</SPAN>
							<HR>
							<BR>
					</TD>
					<TD style="HEIGHT: 36px">&nbsp;</TD>
				</TR>
				<TR>
					<TD style="HEIGHT: 40px"></TD>
					<TD style="WIDTH: 298px; HEIGHT: 40px"><SPAN class="style7">Request extend date:</SPAN></TD>
					<TD style="HEIGHT: 40px"><INPUT id="txtFulfill" onblur="txtFulfill_onblur();" style="FONT-SIZE: medium; WIDTH: 136px; FONT-FAMILY: System; HEIGHT: 21px"
							accessKey="f" type="text" maxLength="50" size="17" name="txtFulfill">&nbsp;(YYYY/MM/DD)</TD>
					<TD style="HEIGHT: 40px"></TD>
				</TR>
				<TR>
					<TD></TD>
					<TD style="WIDTH: 298px"><SPAN class="style7">Margin call amount:</SPAN></TD>
					<TD style="WIDTH: 663px"><INPUT id="txtCurrency" onblur="TxtCurrency_onblur();" style="FONT-SIZE: medium; WIDTH: 136px; FONT-FAMILY: System; HEIGHT: 23px; TEXT-ALIGN: right"
							accessKey="v" type="text" maxLength="19" size="13" name="txtCurrency"><SELECT id="selectCurrency" style="FONT-SIZE: medium; WIDTH: 81px; FONT-FAMILY: System"
							accessKey="c" onchange="SelectCurrency_onchange();" name="selectCurrency">
							<OPTION selected></OPTION>
						</SELECT></TD>
					<TD></TD>
				</TR>
				<TR>
					<TD style="HEIGHT: 22px"></TD>
					<TD colSpan="2" style="HEIGHT: 22px"><FONT face="宋体"><BR>
							<FONT face="Times New Roman">
								<HR>
							</FONT>
							<BR>
							<DIV id="lblMessage" style="DISPLAY: inline; FONT-WEIGHT: bold; FONT-SIZE: medium; WIDTH: 100%; COLOR: red; FONT-FAMILY: System; HEIGHT: 21px"
								align="center"></DIV>
						</FONT>
					</TD>
					<TD style="HEIGHT: 22px"></TD>
				</TR>
				<TR>
					<TD style="HEIGHT: 2px"><FONT face="system"></FONT></TD>
					<TD style="HEIGHT: 2px" align="center" colSpan="2">
						<P>
						<P></P>
						<P><FONT face="system">&nbsp;<FONT face="system"><INPUT id="btnSubmit" style="WIDTH: 80px; HEIGHT: 24px" accessKey="s" onclick="Submit();"
										type="button" value="Submit" name="btnSubmit"><INPUT id="btnReset" style="WIDTH: 70px; HEIGHT: 24px" accessKey="r" onclick="Reset();"
										type="reset" value="Reset" name="btnReset"></FONT></FONT></P>
					</TD>
					<TD style="HEIGHT: 2px"><FONT face="宋体"></FONT></TD>
				</TR>
				<TR>
					<TD><FONT face="system"></FONT></TD>
					<TD style="WIDTH: 298px"><FONT face="system"></FONT></TD>
					<TD style="WIDTH: 663px"><FONT face="system"></FONT></TD>
					<TD><FONT face="system"></FONT></TD>
				</TR>
			</TABLE>
		</center>
	</body>
</HTML>
