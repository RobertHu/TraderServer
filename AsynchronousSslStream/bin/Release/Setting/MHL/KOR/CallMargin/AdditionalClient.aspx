<%@ Page Language="C#" AutoEventWireup="true" Inherits="MHL_KOR_CallMargin_AdditionalClient" Codebehind="AdditionalClient.aspx.cs" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<HTML>
	<HEAD>
		<title>Owner Registration</title>
		<META http-equiv="Content-Type" content="text/html; charset=utf-8">
		<meta content="Microsoft Visual Studio 7.0" name="GENERATOR">
		<meta content="C#" name="CODE_LANGUAGE">
		<meta content="JavaScript" name="vs_defaultClientScript">
		<meta content="http://schemas.microsoft.com/intellisense/ie5" name="vs_targetSchema">
		<script language="jscript" src="../../../Javascript/AdditionalClient.js"></script>
	</HEAD>
	<body onload="Onload();">
		<center>
			<TABLE id="Table1" style="WIDTH: 608px; HEIGHT: 665px" cellSpacing="1" cellPadding="1"
				width="608" border="0">
				<TBODY>
					<TR>
						<TD><FONT face="system">&nbsp;</FONT></TD>
						<TD colSpan="2"><FONT face="system"></FONT>
							<P align="center"><STRONG><U>CHANGE OF OWNER REGISTRATION</U></STRONG>
							</P>
						</TD>
						<TD></TD>
					</TR>
					<TR>
						<TD style="HEIGHT: 167px"><FONT face="system"></FONT></TD>
						<TD colSpan="2" style="HEIGHT: 167px">
							<P><SPAN lang="EN-US" style="FONT-SIZE: 12pt"><SPAN lang="EN-US" style="FONT-SIZE: 12pt"><FONT face="system"></FONT></P>
							<P><BR>
								<TABLE id="Table2" cellSpacing="1" cellPadding="1" width="568" border="0" style="WIDTH: 568px; HEIGHT: 95px">
									<TR>
										<TD style="WIDTH: 134px">Date</TD>
										<TD style="WIDTH: 28px">:</TD>
										<TD>CurrentTradeDay</TD>
									</TR>
									<TR>
										<TD style="WIDTH: 134px"><SPAN lang="EN-US" style="FONT-SIZE: 12pt">To</SPAN></TD>
										<TD style="WIDTH: 28px">:</TD>
										<TD>OrganizationName</TD>
									</TR>
									<TR>
										<TD style="WIDTH: 134px">From</TD>
										<TD style="WIDTH: 28px">:</TD>
										<TD>CustomerName, AccountCode</TD>
									</TR>
									<TR>
										<TD style="WIDTH: 134px">Registrated E-mail</TD>
										<TD style="WIDTH: 28px">:</TD>
										<TD><INPUT id="txtEmail" onblur="txtEmail_onblur();" style="FONT-SIZE: medium; WIDTH: 368px; FONT-FAMILY: System; HEIGHT: 23px; TEXT-ALIGN: left"
												accessKey="e" type="text" maxLength="80" size="56" name="txtEmail"></TD>
									</TR>
								</TABLE>
								</SPAN></SPAN><SPAN lang="EN-US" style="FONT-SIZE: 12pt"><SPAN lang="EN-US" style="FONT-SIZE: 12pt"><SPAN lang="EN-US" style="FONT-SIZE: 12pt"><SPAN lang="EN-US" style="FONT-SIZE: 12pt">
												<HR>
											</SPAN></SPAN></SPAN></SPAN>
							<P><U><STRONG>Original owner information</STRONG></U></P>
						</TD>
						<TD style="HEIGHT: 167px">&nbsp;</TD>
					</TR>
					<TR>
						<TD style="HEIGHT: 11px"></TD>
						<TD style="WIDTH: 166px; HEIGHT: 11px">Corresponding Address:</TD>
						<TD style="HEIGHT: 11px"><INPUT id="txtCorrespondingAddress" style="WIDTH: 400px; HEIGHT: 22px" type="text" maxLength="100"
								size="66" name="txtCorrespondingAddress"></TD>
						<TD style="HEIGHT: 11px"></TD>
					</TR>
					<TR>
						<TD style="HEIGHT: 23px"></TD>
						<TD style="WIDTH: 166px; HEIGHT: 23px">
							New&nbsp;Email address:</TD>
						<TD style="HEIGHT: 23px"><INPUT id="txtRegistratedEmailAddress" style="WIDTH: 400px; HEIGHT: 22px" type="text" maxLength="80"
								size="66" name="txtRegistratedEmailAddress"></TD>
						<TD style="HEIGHT: 23px"></TD>
					</TR>
					<TR>
						<TD style="HEIGHT: 16px"></TD>
						<TD style="WIDTH: 166px; HEIGHT: 16px">Telephone no :</TD>
						<TD style="HEIGHT: 16px"><INPUT id="txtTel" style="WIDTH: 400px; HEIGHT: 22px" type="text" maxLength="18" size="66"
								name="txtTel"></TD>
						<TD style="HEIGHT: 16px"></TD>
					</TR>
					<TR>
						<TD style="HEIGHT: 5px"></TD>
						<TD style="WIDTH: 166px; HEIGHT: 5px">Mobile no :</TD>
						<TD style="HEIGHT: 5px"><INPUT id="txtMobile" style="WIDTH: 400px; HEIGHT: 22px" type="text" maxLength="18" size="66"
								name="txtMobile"></TD>
						<TD style="HEIGHT: 5px"></TD>
					</TR>
					<TR>
						<TD style="HEIGHT: 3px"></TD>
						<TD style="WIDTH: 166px; HEIGHT: 3px">Fax no :</TD>
						<TD style="HEIGHT: 3px"><INPUT id="txtFax" style="WIDTH: 400px; HEIGHT: 22px" type="text" maxLength="18" size="66"
								name="txtFax"></TD>
						<TD style="HEIGHT: 3px"></TD>
					</TR>
					<TR>
						<TD></TD>
						<TD colSpan="2"><FONT size="2">
								<DIV align="center"><SPAN class="style16"><SPAN class="style16">
											<DIV align="center"><SPAN class="style16"> </SPAN>
											</DIV>
											<DIV align="left"><SPAN class="style16"><STRONG style="TEXT-ALIGN: left"><U><BR>
															<FONT size="3">Information of additional&nbsp;owner (1)</FONT> </U></STRONG>
												</SPAN>
											</DIV>
										</SPAN></SPAN>
								</DIV>
							</FONT>
						</TD>
						<TD></TD>
					</TR>
					<TR>
						<TD></TD>
						<TD style="WIDTH: 166px"><SPAN class="style7"><SPAN>Full Name :</SPAN></SPAN></TD>
						<TD><INPUT id="txtFullName1" style="WIDTH: 400px; HEIGHT: 22px" type="text" maxLength="50"
								size="66" name="txtFullName1"></TD>
						<TD></TD>
					</TR>
					<TR>
						<TD></TD>
						<TD style="WIDTH: 166px"><SPAN class="style7"><SPAN class="style7"><SPAN>IC No. :</SPAN></SPAN></SPAN></TD>
						<TD><INPUT id="txtICNo1" style="WIDTH: 400px; HEIGHT: 22px" type="text" maxLength="30" size="66"
								name="txtICNo1"></TD>
						<TD></TD>
					</TR>
					<TR>
						<TD></TD>
						<TD colSpan="2"><FONT size="2"><SPAN class="style16"><STRONG>
										<DIV align="left"><SPAN class="style16"><STRONG><U><BR>
														<FONT size="3">Information of additional&nbsp;owner (2)</FONT> </U></STRONG>
											</SPAN>
										</DIV>
									</STRONG></SPAN></FONT>
						</TD>
						<TD></TD>
					</TR>
					<TR>
						<TD style="HEIGHT: 15px"></TD>
						<TD style="WIDTH: 166px; HEIGHT: 15px"><SPAN class="style7"><SPAN class="style7">Full Name 
									:</SPAN></SPAN></TD>
						<TD style="HEIGHT: 15px"><INPUT id="txtFullName2" style="WIDTH: 400px; HEIGHT: 22px" type="text" maxLength="50"
								size="66" name="txtFullName2"></TD>
						<TD style="HEIGHT: 15px"></TD>
					</TR>
					<TR>
						<TD style="HEIGHT: 5px"></TD>
						<TD style="WIDTH: 166px; HEIGHT: 5px"><SPAN class="style7"><SPAN class="style7"><SPAN class="style7">IC 
										No. :</SPAN></SPAN></SPAN></TD>
						<TD style="HEIGHT: 5px"><INPUT id="txtICNo2" style="WIDTH: 400px; HEIGHT: 22px" type="text" maxLength="30" size="66"
								name="txtICNo2"></TD>
						<TD style="HEIGHT: 5px"></TD>
					</TR>
					<TR>
						<TD style="HEIGHT: 13px"></TD>
						<TD style="HEIGHT: 13px" colSpan="2"><FONT size="2">
								<DIV align="left"><SPAN class="style16"><STRONG style="TEXT-DECORATION: underline"><BR>
											<FONT size="3">Information of additional&nbsp;owner (3)</FONT></STRONG></SPAN></DIV>
							</FONT>
						</TD>
						<TD style="HEIGHT: 13px"></TD>
					</TR>
					<TR>
						<TD></TD>
						<TD style="WIDTH: 166px"><SPAN class="style7"><SPAN class="style7">Full Name :</SPAN></SPAN></TD>
						<TD><INPUT id="txtFullName3" style="WIDTH: 400px; HEIGHT: 22px" type="text" maxLength="50"
								size="66" name="txtFullName3"></TD>
						<TD></TD>
					</TR>
					<TR>
						<TD style="HEIGHT: 2px"></TD>
						<TD style="WIDTH: 166px; HEIGHT: 2px"><SPAN class="style7"><SPAN class="style7"><SPAN class="style7">IC 
										No. :</SPAN></SPAN></SPAN></TD>
						<TD style="HEIGHT: 2px"><FONT face="system"><INPUT id="txtICNo3" style="WIDTH: 400px; HEIGHT: 22px" type="text" maxLength="30" size="66"
									name="txtICNo3"></FONT></TD>
						<TD style="HEIGHT: 2px"></TD>
					</TR>
					<TR>
						<TD style="HEIGHT: 53px"></TD>
						<TD colSpan="2" style="HEIGHT: 53px">
							<P><hr>
							<P></P>
							<P>
							</P>
							<DIV id="lblMessage" style="DISPLAY: inline; FONT-WEIGHT: bold; FONT-SIZE: medium; WIDTH: 100%; COLOR: red; FONT-FAMILY: System; HEIGHT: 21px"
								align="center"><FONT face="system" color="#000000" size="3"></FONT></DIV>
							<FONT face="system"></FONT><FONT face="system"></FONT><FONT face="system"></FONT><FONT face="system">
							</FONT>
						</TD>
						<TD style="HEIGHT: 53px"><FONT face="system"></FONT></TD>
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
						<TD style="WIDTH: 166px"></TD>
						<TD><FONT face="system"></FONT></TD>
						<TD><FONT face="system"></FONT></TD>
					</TR>
				</TBODY>
			</TABLE>
		</center>
	</body>
</HTML>
