<%@ Page Language="C#" AutoEventWireup="true" Inherits="MHL_CHS_CallMargin_PaymentInstruction" Codebehind="PaymentInstruction.aspx.cs" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<HTML>
	<HEAD>
		<title>Payment Instruction</title>
		<META http-equiv="Content-Type" content="text/html; charset=utf-8">
		<meta content="Microsoft Visual Studio 7.0" name="GENERATOR">
		<meta content="C#" name="CODE_LANGUAGE">
		<meta content="JavaScript" name="vs_defaultClientScript">
		<meta content="http://schemas.microsoft.com/intellisense/ie5" name="vs_targetSchema">
		<script language="jscript" src="../../../Javascript/PaymentInstructionPage.js"></script>		
	</HEAD>
	<body onload="Onload();">
		<center>
			<TABLE id="Table1" cellSpacing="1" cellPadding="1" width="589" border="0" style="WIDTH: 589px; HEIGHT: 540px">
				<TBODY>
					<TR>
						<TD style="HEIGHT: 9px"><FONT face="system">&nbsp;</FONT></TD>
						<TD style="HEIGHT: 9px" colSpan="2">
							<P align="center"><STRONG><U>PAYMENT INSTRUCTION</U></STRONG></P>
						</TD>
						<TD style="HEIGHT: 9px"></TD>
					</TR>
					<TR>
						<TD style="HEIGHT: 9px"></TD>
						<TD style="HEIGHT: 157px" colSpan="2"><FONT face="system"></FONT>
							<P><SPAN lang="EN-US" style="FONT-SIZE: 12pt"></SPAN><FONT face="system"><FONT face="system"><BR>
									</FONT>
									<TABLE id="Table2" style="WIDTH: 568px; HEIGHT: 95px" cellSpacing="1" cellPadding="1" width="568"
										border="0">
										<TR>
											<TD style="WIDTH: 132px"><FONT face="Times New Roman">Date</FONT></TD>
											<TD style="WIDTH: 28px"><FONT face="Times New Roman">:</FONT></TD>
											<TD><FONT face="Times New Roman">CurrentTradeDay</FONT></TD>
										</TR>
										<TR>
											<TD style="WIDTH: 132px"><SPAN lang="EN-US" style="FONT-SIZE: 12pt"><FONT face="Times New Roman">To</FONT></SPAN></TD>
											<TD style="WIDTH: 28px"><FONT face="Times New Roman">:</FONT></TD>
											<TD><FONT face="Times New Roman">OrganizationName</FONT></TD>
										</TR>
										<TR>
											<TD style="WIDTH: 132px"><FONT face="Times New Roman">From</FONT></TD>
											<TD style="WIDTH: 28px"><FONT face="Times New Roman">:</FONT></TD>
											<TD><FONT face="Times New Roman">CustomerName, AccountCode</FONT></TD>
										</TR>
										<TR>
											<TD style="WIDTH: 132px"><FONT face="Times New Roman">Registrated E-mail</FONT></TD>
											<TD style="WIDTH: 28px"><FONT face="Times New Roman">:</FONT></TD>
											<TD><INPUT id="txtEmail" onblur="txtEmail_onblur();" style="FONT-SIZE: medium; WIDTH: 368px; FONT-FAMILY: System; HEIGHT: 23px; TEXT-ALIGN: left"
													accessKey="e" type="text" maxLength="80" size="56" name="txtEmail"><FONT face="Times New Roman"></FONT></TD>
										</TR>
									</TABLE>
								</FONT>
							</P>
							<STRONG>This is Client: &nbsp;<INPUT id="ClientRadio1" type="radio" CHECKED name="ClientRadio">(1) 
								&nbsp;<INPUT id="ClientRadio2" type="radio" name="ClientRadio">(2)&nbsp; <INPUT id="ClientRadio3" type="radio" name="ClientRadio">(3)&nbsp;
								<INPUT id="ClientRadio4" type="radio" name="ClientRadio">(4)</STRONG>
							<HR>
						</TD>
						<td style="HEIGHT: 157px">&nbsp;</td>
					</TR>
					<TR>
						<TD style="HEIGHT: 15px"></TD>
						<TD style="WIDTH: 203px; HEIGHT: 15px">Amount:</TD>
						<TD style="HEIGHT: 15px">
							<INPUT id="txtCurrency" onblur="TxtCurrency_onblur();" style="FONT-SIZE: medium; WIDTH: 123px; FONT-FAMILY: System; HEIGHT: 24px; TEXT-ALIGN: right"
								accessKey="v" type="text" maxLength="19" size="15" name="txtCurrency">
							<SELECT id="selectCurrency" style="FONT-SIZE: medium; WIDTH: 74px; FONT-FAMILY: System"
								accessKey="c" onchange="SelectCurrency_onchange();" name="selectCurrency">
								<OPTION selected></OPTION>
							</SELECT></TD>
						<TD style="HEIGHT: 15px"></TD>
					</TR>
					<TR>
						<TD style="HEIGHT: 15px"></TD>
						<TD style="WIDTH: 203px; HEIGHT: 15px">Name of beneficiary:</TD>
						<TD style="HEIGHT: 15px"><TEXTAREA id="txtHolder" style="WIDTH: 400px; HEIGHT: 38px" accessKey="h" name="txtHolder"
								rows="2" cols="67">
							</TEXTAREA></TD>
						<TD style="HEIGHT: 15px"></TD>
					</TR>
					<TR>
						<TD style="HEIGHT: 15px"></TD>
						<TD style="WIDTH: 203px; HEIGHT: 15px">Bank Account No.:</TD>
						<TD style="HEIGHT: 15px">
							<INPUT id="txtBankAccount" style="FONT-SIZE: medium;  WIDTH: 400px;  FONT-FAMILY: System;  HEIGHT: 23px"
								accessKey="a" type="text" maxLength="50" size="88" name="txtBankAccount"></TD>
						<TD style="HEIGHT: 15px"></TD>
					</TR>
					<TR>
						<TD style="HEIGHT: 27px"></TD>
						<TD style="WIDTH: 203px; HEIGHT: 27px">Name of Banker:</TD>
						<TD style="HEIGHT: 27px">
							<INPUT id="txtBankName" style="FONT-SIZE: medium;  WIDTH: 400px;  FONT-FAMILY: System;  HEIGHT: 23px"
								accessKey="n" type="text" maxLength="120" size="88" name="txtBankName"></TD>
						<TD style="HEIGHT: 27px"></TD>
					</TR>
					<TR>
						<TD style="HEIGHT: 15px"></TD>
						<TD style="WIDTH: 203px; HEIGHT: 15px">Banker's Address:</TD>
						<TD style="HEIGHT: 15px"><TEXTAREA id="txtBankerAddress" style="WIDTH: 400px; HEIGHT: 38px" accessKey="b" name="txtBankerAddress"
								rows="2" cols="67">
							</TEXTAREA></TD>
						<TD style="HEIGHT: 15px"></TD>
					</TR>
					<TR>
						<TD style="HEIGHT: 15px"></TD>
						<TD style="WIDTH: 203px; HEIGHT: 15px">Swift Code:
						</TD>
						<TD style="HEIGHT: 15px">
							<INPUT id="txtSwiftCode" style="FONT-SIZE: medium;  WIDTH: 400px;  FONT-FAMILY: System;  HEIGHT: 23px"
								accessKey="r" type="text" maxLength="50" size="88" name="txtSwiftCode"></TD>
						<TD style="HEIGHT: 15px"></TD>
					</TR>
					<TR>
						<TD style="HEIGHT: 15px"></TD>
						<TD style="WIDTH: 203px; HEIGHT: 15px">Remarks:</TD>
						<TD style="HEIGHT: 15px"><TEXTAREA id="txtRemarks" style="WIDTH: 400px; HEIGHT: 38px" accessKey="r" name="txtRemarks"
								rows="2" cols="67">
							</TEXTAREA></TD>
						<TD style="HEIGHT: 15px"><FONT face="system"></FONT></TD>
					</TR>
					<TR>
						<TD style="HEIGHT: 46px"><FONT face="system"></FONT></TD>
						<TD style="HEIGHT: 46px" colSpan="2"><FONT face="system"></FONT><FONT face="system"><BR>
							</FONT>
							<HR>
							<DIV id="lblMessage" style="DISPLAY: inline; FONT-WEIGHT: bold; FONT-SIZE: medium; WIDTH: 100%; COLOR: red; FONT-FAMILY: System; HEIGHT: 21px"
								align="center"><FONT face="system"></FONT><FONT face="system"></FONT><FONT face="system"></FONT><FONT face="system"></FONT><FONT face="system"></FONT></DIV>
							<FONT face="system"></FONT>
						</TD>
						<TD style="HEIGHT: 46px"><FONT face="system"></FONT></TD>
					</TR>
					<TR>
						<TD><FONT face="system"></FONT></TD>
						<TD colSpan="2" align="center">
							<INPUT id="btnSubmit" style="WIDTH: 74px; HEIGHT: 24px" accessKey="s" onclick="Submit();"
								type="button" value="Submit" name="btnSubmit"><INPUT id="btnReset" onclick="Reset();" style="WIDTH: 70px; HEIGHT: 24px" accessKey="r"
								type="reset" value="Reset">
						</TD>
						<TD></TD>
					</TR>
					<TR>
						<TD></TD>
						<TD style="WIDTH: 203px"><FONT face="system"></FONT></TD>
						<TD><FONT face="system"></FONT></TD>
						<TD></TD>
					</TR>
				</TBODY>
			</TABLE>
		</center>
	</body>
</HTML>
