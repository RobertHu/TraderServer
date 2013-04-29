<%@ Page Language="C#" AutoEventWireup="true" Inherits="MHL_JPN_CallMargin_Agent" Codebehind="Agent.aspx.cs" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<HTML>
	<HEAD>
		<title>Agent Registration</title>
		<META http-equiv="Content-Type" content="text/html; charset=utf-8">
		<meta name="GENERATOR" Content="Microsoft Visual Studio 7.0">
		<meta name="CODE_LANGUAGE" Content="C#">
		<meta name="vs_defaultClientScript" content="JavaScript">
		<meta name="vs_targetSchema" content="http://schemas.microsoft.com/intellisense/ie5">
		<script language="jscript" src="../../../Javascript/AgentPage.js"></script>
	</HEAD>
	<body onload="Onload();">
		<center>
			<TABLE id="Table1" cellSpacing="1" cellPadding="1" width="608" border="0" style="WIDTH: 608px; HEIGHT: 481px">
				<TBODY>
					<TR>
						<TD><FONT face="system">&nbsp;</FONT></TD>
						<TD colSpan="2"><FONT face="system"></FONT>
							<P align="center"><STRONG><U>CHANGE OF AGENT REGISTRATION</U></STRONG>
							</P>
						</TD>
						<TD></TD>
					</TR>
					<TR>
						<TD><FONT face="system"></FONT></TD>
						<TD colSpan="2"><BR>
							<TABLE id="Table2" style="WIDTH: 568px; HEIGHT: 95px" cellSpacing="1" cellPadding="1" width="568"
								border="0">
								<TR>
									<TD style="WIDTH: 138px">Date</TD>
									<TD style="WIDTH: 28px">:</TD>
									<TD>CurrentTradeDay</TD>
								</TR>
								<TR>
									<TD style="WIDTH: 138px"><SPAN lang="EN-US" style="FONT-SIZE: 12pt">To</SPAN></TD>
									<TD style="WIDTH: 28px">:</TD>
									<TD>OrganizationName</TD>
								</TR>
								<TR>
									<TD style="WIDTH: 138px">From</TD>
									<TD style="WIDTH: 28px">:</TD>
									<TD>CustomerName, AccountCode</TD>
								</TR>
								<TR>
									<TD style="WIDTH: 138px">Registrated E-mail</TD>
									<TD style="WIDTH: 28px">:</TD>
									<TD><INPUT id="txtEmail" onblur="txtEmail_onblur();" style="FONT-SIZE: medium; WIDTH: 368px; FONT-FAMILY: System; HEIGHT: 23px; TEXT-ALIGN: left"
											accessKey="e" type="text" maxLength="80" size="56" name="txtEmail"></TD>
								</TR>
							</TABLE>
							<HR>
						</TD>
						<TD>&nbsp;</TD>
					</TR>
					<TR>
						<TD style="HEIGHT: 30px"></TD>
						<TD style="WIDTH: 171px; HEIGHT: 30px"><SPAN class="style7">Effective Date: </SPAN>
						</TD>
						<TD style="HEIGHT: 30px"><INPUT id="txtDateReply" onblur="txtDateReply_onblur();" style="FONT-SIZE: medium; WIDTH: 128px; FONT-FAMILY: System; HEIGHT: 24px"
								type="text" maxLength="50" size="16" name="txtDateReply">&nbsp;(YYYY/MM/DD)</TD>
						<TD style="HEIGHT: 30px"></TD>
					</TR>
					<TR>
						<TD></TD>
						<TD colSpan="2"><FONT size="2">
								<DIV align="left"><SPAN class="style16"><STRONG><U><BR>
												<FONT size="3">Previous Agent Information</FONT></U></STRONG></SPAN></DIV>
							</FONT>
						</TD>
						<TD></TD>
					</TR>
					<TR>
						<TD></TD>
						<TD style="WIDTH: 171px"><SPAN class="style7">Agent Code:</SPAN></TD>
						<TD><INPUT id="txtPreviousAgentCode" style="WIDTH: 400px; HEIGHT: 22px" type="text" maxLength="20"
								size="66" name="txtPreviousAgentCode"></TD>
						<TD></TD>
					</TR>
					<TR>
						<TD></TD>
						<TD style="WIDTH: 171px"><SPAN class="style7"><SPAN class="style7">Agent Name:</SPAN></SPAN></TD>
						<TD><INPUT id="txtPreviousAgentName" style="WIDTH: 400px; HEIGHT: 22px" type="text" maxLength="20"
								size="66" name="txtPreviousAgentName"></TD>
						<TD></TD>
					</TR>
					<TR>
						<TD style="HEIGHT: 27px"></TD>
						<TD style="HEIGHT: 27px" colSpan="2">
							<P align="center"><FONT size="2"><SPAN class="style16"><STRONG></P>
							<DIV align="left"><SPAN class="style16"><U><BR>
										<FONT size="3">New Agent Information</FONT></U></SPAN> </STRONG></SPAN></FONT></DIV>
						</TD>
						<TD style="HEIGHT: 27px"></TD>
					</TR>
					<TR>
						<TD style="HEIGHT: 29px"></TD>
						<TD style="WIDTH: 171px; HEIGHT: 29px"><SPAN class="style7"><SPAN class="style7">Agent 
									Code:</SPAN></SPAN></TD>
						<TD style="HEIGHT: 29px"><INPUT id="txtNewAgentCode" style="FONT-SIZE: medium; WIDTH: 400px; FONT-FAMILY: System; HEIGHT: 23px"
								accessKey="o" type="text" maxLength="20" size="66" name="txtNewAgentCode"></TD>
						<TD style="HEIGHT: 29px"></TD>
					</TR>
					<TR>
						<TD></TD>
						<TD style="WIDTH: 171px"><SPAN class="style7"><SPAN class="style7">Agent Name:</SPAN></SPAN></TD>
						<TD><INPUT id="txtNewAgentName" style="FONT-SIZE: medium; WIDTH: 400px; FONT-FAMILY: System; HEIGHT: 26px"
								accessKey="h" type="text" maxLength="20" size="66" name="txtNewAgentName"></TD>
						<TD></TD>
					</TR>
					<TR>
						<TD></TD>
						<TD style="WIDTH: 171px"><SPAN class="style7">IC No. :</SPAN></TD>
						<TD><INPUT id="txtNewAgentICNo" style="WIDTH: 400px; HEIGHT: 22px" type="text" maxLength="30"
								size="66" name="txtNewAgentICNo"></TD>
						<TD></TD>
					</TR>
					<TR>
						<TD></TD>
						<TD colSpan="2"><FONT face="system"></FONT><FONT face="system"></FONT>
							<DIV id="lblMessage" style="DISPLAY: inline; FONT-WEIGHT: bold; FONT-SIZE: medium; WIDTH: 100%; COLOR: red; FONT-FAMILY: System; HEIGHT: 21px"
								align="center"><FONT face="system"></FONT></DIV>
							<FONT face="system"></FONT>
						</TD>
						<TD><FONT face="system"></FONT></TD>
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
						<TD style="WIDTH: 171px"></TD>
						<TD><FONT face="system"></FONT></TD>
						<TD></TD>
					</TR>
				</TBODY>
			</TABLE>
		</center>
	</body>
</HTML>
