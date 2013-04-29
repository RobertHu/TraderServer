<%@ Page Language="C#" AutoEventWireup="true" Inherits="MHL_KOR_CallMargin_FundTransfer" Codebehind="FundTransfer.aspx.cs" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<HTML>
	<HEAD>
		<title>Fund Transfer</title>
		<META http-equiv="Content-Type" content="text/html; charset=utf-8">
		<meta name="GENERATOR" Content="Microsoft Visual Studio 7.0">
		<meta name="CODE_LANGUAGE" Content="C#">
		<meta name="vs_defaultClientScript" content="JavaScript">
		<meta name="vs_targetSchema" content="http://schemas.microsoft.com/intellisense/ie5">
		<script language="jscript" src="../../../Javascript/FundTransferPage.js"></script>
	</HEAD>
	<body onload="Onload();">
		<center>
			<TABLE id="Table1" cellSpacing="1" cellPadding="1" width="621" border="0" style="WIDTH: 621px; HEIGHT: 477px">
				<TBODY>
					<TR>
						<TD><FONT face="system">&nbsp;</FONT></TD>
						<TD colSpan="2"><FONT face="system"></FONT>
							<P align="center"><STRONG><U><U>FUND TRANSFER </U></U></STRONG>
							</P>
						</TD>
						<TD><FONT face="system"></FONT></TD>
					</TR>
					<TR>
						<TD><FONT face="system"></FONT></TD>
						<TD colSpan="2">
							<P><SPAN lang="EN-US" style="FONT-SIZE: 12pt; mso-bidi-font-weight: normal"><FONT face="system"><BR>
									</FONT>
									<TABLE id="Table2" style="WIDTH: 584px; HEIGHT: 95px" cellSpacing="1" cellPadding="1" width="584"
										border="0">
										<TR>
											<TD style="WIDTH: 119px">Date</TD>
											<TD style="WIDTH: 28px">:</TD>
											<TD>CurrentTradeDay</TD>
										</TR>
										<TR>
											<TD style="WIDTH: 119px"><SPAN lang="EN-US" style="FONT-SIZE: 12pt">To</SPAN></TD>
											<TD style="WIDTH: 28px">:</TD>
											<TD>OrganizationName</TD>
										</TR>
										<TR>
											<TD style="WIDTH: 119px">From</TD>
											<TD style="WIDTH: 28px">:</TD>
											<TD>CustomerName, AccountCode</TD>
										</TR>
										<TR>
											<TD style="WIDTH: 119px">Registrated E-mail</TD>
											<TD style="WIDTH: 28px">:</TD>
											<TD><INPUT id="txtEmail" onblur="txtEmail_onblur();" style="FONT-SIZE: medium; WIDTH: 368px; FONT-FAMILY: System; HEIGHT: 23px; TEXT-ALIGN: left"
													accessKey="e" type="text" maxLength="80" size="56" name="txtEmail"></TD>
										</TR>
									</TABLE>
							</P>
							<HR>
							</SPAN></TD>
						<TD>&nbsp;</TD>
					</TR>
					<TR>
						<TD style="HEIGHT: 30px"></TD>
						<TD style="WIDTH: 152px; HEIGHT: 30px"><SPAN class="style7">Effective Date: </SPAN>
						</TD>
						<TD style="HEIGHT: 30px"><INPUT id="txtDateReply" onblur="txtDateReply_onblur();" style="FONT-SIZE: medium; WIDTH: 128px; FONT-FAMILY: System; HEIGHT: 24px"
								type="text" maxLength="50" size="16" name="txtDateReply">&nbsp;(YYYY/MM/DD)</TD>
						<TD style="HEIGHT: 30px"></TD>
					</TR>
					<TR>
						<TD></TD>
						<TD style="WIDTH: 152px"><SPAN class="style7">Transfer Amount:</SPAN></TD>
						<TD><INPUT id="txtCurrency" onblur="TxtCurrency_onblur();" style="FONT-SIZE: medium; WIDTH: 128px; FONT-FAMILY: System; HEIGHT: 23px; TEXT-ALIGN: right"
								accessKey="v" type="text" maxLength="19" size="16" name="txtCurrency"><SELECT id="selectCurrency" style="FONT-SIZE: medium; WIDTH: 85px; FONT-FAMILY: System"
								accessKey="c" onchange="SelectCurrency_onchange();" name="selectCurrency">
								<OPTION selected></OPTION>
							</SELECT></TD>
						<TD></TD>
					</TR>
					<TR>
						<TD style="HEIGHT: 27px"></TD>
						<TD style="HEIGHT: 27px" colSpan="2">
							<P align="left"><FONT size="2"><SPAN class="style16"><STRONG><U><BR>
												<FONT size="3">Transfer To</FONT></U></STRONG></SPAN></FONT></P>
						</TD>
						<TD style="HEIGHT: 27px"></TD>
					</TR>
					<TR>
						<TD></TD>
						<TD style="WIDTH: 152px"><SPAN class="style7">Trading Account No.:</SPAN></TD>
						<TD><INPUT id="txtOtherAccount" style="FONT-SIZE: medium; WIDTH: 432px; FONT-FAMILY: System; HEIGHT: 23px"
								accessKey="o" type="text" maxLength="50" size="66" name="txtOtherAccount"></TD>
						<TD></TD>
					</TR>
					<TR>
						<TD></TD>
						<TD style="WIDTH: 152px"><SPAN class="style7">Account Holder Name:</SPAN></TD>
						<TD><INPUT id="txtHolder" style="FONT-SIZE: medium; WIDTH: 432px; FONT-FAMILY: System; HEIGHT: 26px"
								accessKey="h" type="text" maxLength="50" size="66" name="txtHolder"></TD>
						<TD></TD>
					</TR>
					<TR>
						<TD style="HEIGHT: 46px"></TD>
						<TD colSpan="2" style="HEIGHT: 46px"><FONT face="system"></FONT><FONT face="system"></FONT><FONT face="system"><BR>
							</FONT>
							<HR>
							<DIV id="lblMessage" style="DISPLAY: inline; FONT-WEIGHT: bold; FONT-SIZE: medium; WIDTH: 100%; COLOR: red; FONT-FAMILY: System; HEIGHT: 21px"
								align="center"><FONT face="system"></FONT></DIV>
							<FONT face="system"></FONT>
						</TD>
						<TD style="HEIGHT: 46px"><FONT face="system"></FONT></TD>
					</TR>
					<TR>
						<TD style="HEIGHT: 26px"></TD>
						<TD style="HEIGHT: 26px" colSpan="2">
							<P align="center"><INPUT id="btnSubmit" style="WIDTH: 73px; HEIGHT: 24px" accessKey="s" onclick="Submit();"
									type="button" value="Submit" name="btnSubmit"><INPUT id="btnReset" style="WIDTH: 70px; HEIGHT: 24px" accessKey="r" onclick="Reset();"
									type="reset" value="Reset" name="btnReset"></P>
						</TD>
						<TD style="HEIGHT: 26px"></TD>
					</TR>
					<TR>
						<TD></TD>
						<TD style="WIDTH: 152px"></TD>
						<TD><FONT face="system"></FONT></TD>
						<TD></TD>
					</TR>
				</TBODY>
			</TABLE>
		</center>
	</body>
</HTML>
