<%@ Page language="c#" Inherits="iExchange.TradingConsole.MHL.JPN.Document.PhysicalDelivery" Codebehind="PhysicalDelivery.aspx.cs" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<HTML>
	<HEAD>
		<title>Gold Bullion Contracts traded as "Actual Delivery" Contracts</title>
		<meta content="Microsoft Visual Studio .NET 7.1" name="GENERATOR">
		<meta content="C#" name="CODE_LANGUAGE">
		<meta content="JavaScript" name="vs_defaultClientScript">
		<meta content="http://schemas.microsoft.com/intellisense/ie5" name="vs_targetSchema">
		</SCRIPT>
		<%
		Response.Write( "<script>\r\n" );
		Response.Write( "var path = \"" );
		Response.Write( (string)Page.Session["Path"] );
		Response.Write( "\";\r\n" );
		Response.Write( "</script>\r\n" );
		%>
		<script language="javascript" src="../../../Javascript/Document.js"></script>
		<script language="javascript" src="../../../Setting/WindowSize.js"></script>
	</HEAD>
	<body onload="OnloadContent();">
		<!--OrganizationCode,OrganizationName,OrganizationPhone,OrganizationEmail use program to change,user does not allow to change them.-->
		<TABLE id="Table1" style="WIDTH: 100%" cellSpacing="1" cellPadding="1" width="714" border="0">
			<TR>
				<TD style="HEIGHT: 1px"></TD>
				<TD style="HEIGHT: 1px">
				</TD>
				<TD style="HEIGHT: 1px"></TD>
			</TR>
			<TR>
				<TD style="HEIGHT: 36px"></TD>
				<TD valign="top" style="HEIGHT: 36px"><b style='mso-bidi-font-weight:normal'><span lang="EN-US" style="FONT-SIZE:8pt;COLOR:black;FONT-FAMILY:'Times New Roman';mso-fareast-font-family:PMingLiU;mso-font-kerning:1.0pt;mso-ansi-language:EN-US;mso-fareast-language:ZH-TW;mso-bidi-language:AR-SA">RISK 
							WARNING: YOU ARE ENTERING INTO A PROCESS WHERE YOU WANT TO TRADE GOLD BULLION 
							FOR PHYSICAL DELIVERY ¨C THERE MAY BE A HIGH COST OF REVERSING A TRANSACTION 
							THAT IS NOT WHAT YOU HAVE INTENDED.</span></b></TD>
				<TD style="HEIGHT: 36px"></TD>
			</TR>
			<TR>
				<TD></TD>
				<TD id="tdContent" valign="top">
					<p class="MsoNormal" style='TEXT-JUSTIFY:inter-ideograph;LINE-HEIGHT:12pt;TEXT-ALIGN:justify;mso-line-height-rule:exactly'><i style='mso-bidi-font-style:
normal'><span lang="EN-US">This is the web content displayed on the E-Trading System when a Customer selects 
								and clicks the button for trading in Gold Bullion for physical delivery 
								(whether it is for delivery spot or for a certain forward date):<o:p></o:p></span></i></p>
					<p class="MsoNormal" style='TEXT-JUSTIFY:inter-ideograph;LINE-HEIGHT:12pt;TEXT-ALIGN:justify;mso-line-height-rule:exactly'><![if 
						!supportEmptyParas]><![endif]><span lang="EN-US">Dear Customer,<o:p></o:p></span></p>
					<p class="MsoNormal" style='TEXT-JUSTIFY:inter-ideograph;LINE-HEIGHT:12pt;TEXT-ALIGN:justify;mso-line-height-rule:exactly'><![if 
						!supportEmptyParas]><![endif]><span lang="EN-US">Welcome to 
							the&nbsp;OrganizationName</span><span lang="EN-US">&nbsp;(OrganizationCode) 
							department of dealing in Gold Bullion for physical delivery. We are most 
							pleased to be of service to you on facilitating your instruction to purchase 
							(or sale) of Gold Bullion for physical delivery.<o:p></o:p></span></p>
					<p class="MsoNormal" style='TEXT-JUSTIFY:inter-ideograph;LINE-HEIGHT:12pt;TEXT-ALIGN:justify;mso-line-height-rule:exactly'><![if 
						!supportEmptyParas]><![endif]><span lang="EN-US">First of all, we would like to 
							thank you for choosing OrganizationCode to trade in Gold Bullion for physical 
							delivery.<o:p></o:p></span></p>
					<p class="MsoNormal" style='TEXT-JUSTIFY:inter-ideograph;LINE-HEIGHT:12pt;TEXT-ALIGN:justify;mso-line-height-rule:exactly'><![if 
						!supportEmptyParas]><![endif]><span lang="EN-US">Before you proceed further from 
							here, please allow us to bring to your attention the following important notice 
							and reminders of the terms for dealing in Gold Bullion for physical delivery.<o:p></o:p></span></p>
					<p class="MsoNormal" style='TEXT-JUSTIFY:inter-ideograph;LINE-HEIGHT:12pt;TEXT-ALIGN:justify;mso-line-height-rule:exactly'><![if 
						!supportEmptyParas]><![endif]><![if !supportEmptyParas]><![endif]><span lang="EN-US">We 
							would like to take this opportunity to inform you that:<o:p></o:p></span></p>
					<p class="MsoNormal" style='TEXT-JUSTIFY:inter-ideograph;LINE-HEIGHT:12pt;TEXT-ALIGN:justify;mso-line-height-rule:exactly'><![if 
						!supportEmptyParas]><![endif]><![if !supportLists]><span lang="EN-US" style='FONT-FAMILY:Wingdings'>l<span style="FONT:7pt 'Times New Roman'">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
							</span></span><![endif]><span lang="EN-US">On seeing this notice, you have 
							selected the button to request for trading in Gold Bullion for physical 
							delivery. If you have selected this button by mistake, you may click the 
							following button to exit from this request [click here for EXITING trade 
							request for physical delivery].<o:p></o:p></span></p>
					<p class="MsoNormal" style='TEXT-JUSTIFY:inter-ideograph;LINE-HEIGHT:12pt;TEXT-ALIGN:justify;mso-line-height-rule:exactly'><![if 
						!supportEmptyParas]><![endif]><![if !supportLists]><span lang="EN-US" style='FONT-FAMILY:Wingdings'>l<span style="FONT:7pt 'Times New Roman'">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
							</span></span><![endif]><span lang="EN-US">By electing for trading in Gold 
							Bullion for physical delivery and upon execution of the contract, the buyer 
							will assume the obligation to take physical delivery while the seller will 
							assume the obligation to make physical delivery of the Gold Bullion transacted.<o:p></o:p></span></p>
					<p class="MsoNormal" style='TEXT-JUSTIFY:inter-ideograph;LINE-HEIGHT:12pt;TEXT-ALIGN:justify;mso-line-height-rule:exactly'><![if 
						!supportEmptyParas]><![endif]><![if !supportLists]><span lang="EN-US" style='FONT-FAMILY:Wingdings'>l<span style="FONT:7pt 'Times New Roman'">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
							</span></span><![endif]><span lang="EN-US">According to the terms of our Gold 
							Bullion Trading Term Sheet, the location for physical delivery is London. The 
							seller has the obligation to ship Gold Bullion to a Designated Bank while the 
							buyer shall have to take the shipment from the same.<o:p></o:p></span></p>
					<p class="MsoNormal" style='TEXT-JUSTIFY:inter-ideograph;LINE-HEIGHT:12pt;TEXT-ALIGN:justify;mso-line-height-rule:exactly'><![if 
						!supportEmptyParas]><![endif]><![if !supportLists]><span lang="EN-US" style='FONT-FAMILY:Wingdings'>l<span style="FONT:7pt 'Times New Roman'">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
							</span></span><![endif]><span lang="EN-US">For protection of either Party to 
							this transaction, both Parties acknowledge and agree that (in additional to the 
							specific terms and conditions stated below) the actual change in hand of the 
							Gold Bullion contracted for may take place within a period being not less than 
							two business days (days on which banks in London are open for business) to not 
							more than ten business days from the day of trading.<o:p></o:p></span></p>
					<p class="MsoNormal" style='TEXT-JUSTIFY:inter-ideograph;LINE-HEIGHT:12pt;TEXT-ALIGN:justify;mso-line-height-rule:exactly'><![if 
						!supportEmptyParas]><![endif]><![if !supportLists]><span lang="EN-US" style='FONT-FAMILY:Wingdings'>l<span style="FONT:7pt 'Times New Roman'">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
							</span></span><![endif]><span lang="EN-US">Both the Contract Price (the amount 
							of fund required of the buyer to pay to the seller for the Gold Bullion) and 
							the Contract Amount (the amount of Gold Bullion required of the seller to 
							deliver to the buyer) shall be placed with the Designated Bank in London and 
							held by OrganizationCode in trust for your account for the above period until 
							the actual delivery has taken effect. In the case where OrganizationCode is the 
							seller, the Designated Bank will hold the Contract Amount for OrganizationCode 
							held in trust for you until OrganizationCode has received the Contract Price 
							from you, by which event, the Designated Bank will release the Contract Amount 
							to you. In the case where you are the seller, the Designated Bank will hold the 
							Contract Amount for you for release to OrganizationCode upon receipt payment of 
							the Contract Price by OrganizationCode. <span style="mso-spacerun: yes">&nbsp;</span><o:p></o:p></span></p>
					<p class="MsoNormal" style='TEXT-JUSTIFY:inter-ideograph;LINE-HEIGHT:12pt;TEXT-ALIGN:justify;mso-line-height-rule:exactly'><![if 
						!supportEmptyParas]><![endif]><![if !supportLists]><span lang="EN-US" style='FONT-FAMILY:Wingdings'>l<span style="FONT:7pt 'Times New Roman'">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
							</span></span><![endif]><span lang="EN-US">For protection of your interest, you 
							are required to identify your identity by first making a telephone call to the 
							dealing department of OrganizationCode at the following number 
							OrganizationPhone, you shall then verbally inform our Gold Bullion dealing 
							manager: (a) your account name; (b) your account number; (c) your personal ID 
							(passport number or identity card number); (d) the quantity of Gold Bullion you 
							wish to buy (or sell); and (e) you may be required to provide an answer that 
							should match with the personal data you have submitted as a part of the Client 
							Information Statement given to OrganizationCode for identification purpose.<o:p></o:p></span></p>
					<p class="MsoNormal" style='TEXT-JUSTIFY:inter-ideograph;LINE-HEIGHT:12pt;TEXT-ALIGN:justify;mso-line-height-rule:exactly'><![if 
						!supportEmptyParas]><![endif]><![if !supportLists]><span lang="EN-US" style='FONT-FAMILY:Wingdings'>l<span style="FONT:7pt 'Times New Roman'">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
							</span></span><![endif]><span lang="EN-US">For further authentication and 
							confirmation of your true identity, you are required to send an e-mail message 
							carrying the details as listed above to the following dedicated e-mail address 
							at OrganizationCode (<a href="mailto:OrganizationEmail"><u style='text-underline:white'><span style='COLOR:black;TEXT-DECORATION:none;text-underline:none'>OrganizationEmail</span></u></a>). 
							The e-mail message that you send in reply must be sent from the e-mail address 
							under your registration that you have stated and declared to us in the Client 
							Information Statement form as you have submitted to us on returning the Gold 
							Bullion Trading Term Sheet.<o:p></o:p></span></p>
					<p class="MsoNormal" style='TEXT-JUSTIFY:inter-ideograph;LINE-HEIGHT:12pt;TEXT-ALIGN:justify;mso-line-height-rule:exactly'><![if 
						!supportEmptyParas]><![endif]><![if !supportLists]><span lang="EN-US" style='FONT-FAMILY:Wingdings'>l<span style="FONT:7pt 'Times New Roman'">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
							</span></span><![endif]><span lang="EN-US">After the identification and 
							authentication check are completed, our dealing desk will reply to you by 
							sending an e-mail message, informing you that OrganizationCode is ready to make 
							a quote on Gold Bullion for dealing (for physical delivery).<o:p></o:p></span></p>
					<p class="MsoNormal" style='TEXT-JUSTIFY:inter-ideograph;LINE-HEIGHT:12pt;TEXT-ALIGN:justify;mso-line-height-rule:exactly'><![if 
						!supportEmptyParas]><![endif]><![if !supportEmptyParas]><![endif]><span lang="EN-US">YOU 
							BUY GOLD BULLION FOR PHYSICAL DELIVERY<o:p></o:p></span></p>
					<p class="MsoNormal" style='TEXT-JUSTIFY:inter-ideograph;LINE-HEIGHT:12pt;TEXT-ALIGN:justify;mso-line-height-rule:exactly'><![if 
						!supportEmptyParas]><![endif]><span lang="EN-US">If you buy Gold Bullion for 
							physical delivery (for forward delivery on a certain specified date not less 
							than two business days band no more than ten business days from the trade day), 
							then<o:p></o:p></span></p>
					<![if !supportEmptyParas]><![endif]><![if !supportLists]>
					<p class="MsoNormal" style='TEXT-JUSTIFY:inter-ideograph;LINE-HEIGHT:12pt;TEXT-ALIGN:justify;mso-line-height-rule:exactly'><span lang="EN-US">1.<span style="FONT:7pt 'Times New Roman'">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
							</span></span><![endif]><span lang="EN-US">You shall be required to have, for 
							the amount of Gold Bullion you want to buy, the full amount of the required 
							funds in U.S. dollars in readily available funds in your account held with 
							OrganizationCode before you enter your buy order. The relevant amount of funds 
							shall be debited directly from your account upon the conclusion of the trade 
							and the issuance of a Contract Note. OrganizationCode <span style="mso-spacerun: yes">
								&nbsp;</span>may, but is not obliged to, refuse your request for entering 
							into transaction in Gold Bullion for physical delivery if in OrganizationCode¡¯s 
							opinion and absolute discretion that there is not sufficient funds in your 
							account for the purchase.<![if !supportLists]><BR>
						</span><span lang="EN-US">2.<span style="FONT:7pt 'Times New Roman'">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
							</span></span><![endif]><span lang="EN-US">On the settlement date, you will 
							take actual delivery of physical Gold Bullion in London (or as the case may be, 
							a depository receipt representing the same) from a Designated Bank and you 
							shall be fully responsible for the costs of transportation (if you wish to move 
							the Gold Bullion away from the Designated Bank), or the costs of custody (if 
							you wish to keep the Gold Bullion in the Designated Bank). OrganizationCode 
							shall be released of all its obligations (as per the terms of the Gold Bullion 
							Trading Term Sheet) by delivering the Gold Bullion to the Designated Bank.<BR>
						</span><![if !supportLists]><span lang="EN-US">3.<span style="FONT:7pt 'Times New Roman'">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
							</span></span><![endif]><span lang="EN-US">If you want to have the Gold Bullion 
							verified by an assayer, you will be required to pay the fees for assaying.<BR>
						</span><![if !supportLists]><span lang="EN-US">4.<span style="FONT:7pt 'Times New Roman'">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
							</span></span><![endif]><span lang="EN-US">OrganizationCode may charge extra 
							commissions and any out-of-pocket expenses to you by direct debit from your 
							account where such extra commissions, costs or expenses are not included in the 
							agreed Contract Price for transaction.<BR>
						</span><![if !supportLists]><span lang="EN-US">5.<span style="FONT:7pt 'Times New Roman'">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
							</span></span><![endif]><span lang="EN-US">You will pay sales tax (if any) and 
							any other levies (where applicable) in relation to assuming the ownership of 
							the Gold Bullion delivered.</span><![if !supportEmptyParas]><![endif]><![if 
						!supportEmptyParas]><![endif]><o:p></o:p></p>
					<p class="MsoNormal" style='TEXT-JUSTIFY:inter-ideograph;LINE-HEIGHT:12pt;TEXT-ALIGN:justify;mso-line-height-rule:exactly'><span lang="EN-US">YOU 
							SELL GOLD BULLION FOR PHYSICAL DELIVERY<o:p></o:p></span></p>
					<![if !supportEmptyParas]><![endif]>
					<P class="MsoNormal" style="TEXT-JUSTIFY: inter-ideograph; LINE-HEIGHT: 12pt; TEXT-ALIGN: justify; mso-line-height-rule: exactly"><span lang="EN-US">If 
							you sell Gold Bullion for physical delivery (for settlement on spot delivery or 
							for forward delivery on a certain specified date), then<![if 
							!supportEmptyParas]><![endif]><![if !supportLists]></span></P>
					<P class="MsoNormal" style="TEXT-JUSTIFY: inter-ideograph; LINE-HEIGHT: 12pt; TEXT-ALIGN: justify; mso-line-height-rule: exactly"><SPAN lang="EN-US"></SPAN><span lang="EN-US">1.<span style="FONT:7pt 'Times New Roman'">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
							</span></span><![endif]><span lang="EN-US">You shall be required to have, for 
							the amount of Gold Bullion you want to sell, the full amount of the Gold 
							Bullion held in absolute control with OrganizationCode before you enter your 
							sell order. The relevant Gold Bullion shall be debited directly from your 
							account upon the conclusion of the trade and the issuance of a Contract Note. 
							OrganizationCode may, but is not obliged to, refuse your request for entering 
							into transaction in Gold Bullion for physical delivery if in OrganizationCode¡¯s 
							opinion and absolute discretion that there is not sufficient Gold Bullion in 
							your account (held in absolute control of OrganizationCode <span style="mso-spacerun: yes">
								&nbsp;</span>for the sale.<BR>
						</span><![if !supportLists]><span lang="EN-US">2.<span style="FONT:7pt 'Times New Roman'">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
							</span></span><![endif]><span lang="EN-US">OrganizationCode <span style="mso-spacerun:
yes">&nbsp;</span>may, but is not obliged to, instead of your having the relevant amount of Gold Bullion, 
							accept from you a sale order of Gold Bullion for physical delivery if in 
							OrganizationCode¡¯s <span style="mso-spacerun: yes">&nbsp;</span>absolute 
							opinion that you have sufficient amount of funds to cover the sale.<BR>
						</span><![if !supportLists]><span lang="EN-US">3.<span style="FONT:7pt 'Times New Roman'">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
							</span></span><![endif]><span lang="EN-US">All sales by you will be for 
							physical delivery within the delivery period as stated above, i.e., delivery of 
							and payment for the Gold Bullion will be effected on the day being no less than 
							two business days and no more than ten business days from the day of 
							transaction.<BR>
						</span><![if !supportLists]><span lang="EN-US">4.<span style="FONT:7pt 'Times New Roman'">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
							</span></span><![endif]><span lang="EN-US">OrganizationCode <span style="mso-spacerun:
yes">&nbsp;</span>may charge extra commissions and any out-of-pocket expenses to you by direct debit from 
							your account where such extra commission, costs and expenses are not included 
							in the agreed Contract Price for transaction.<![if !supportLists]><BR>
						</span><span lang="EN-US">5.<span style="FONT:7pt 'Times New Roman'">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
							</span></span><![endif]><span lang="EN-US">Payment by OrganizationCode to you 
							of the relevant sum in U.S. dollars (the relevant amount of Adjusted Contract 
							Price, as defined in the Gold Bullion Trading Term Sheet) will be credited to 
							your Gold Bullion trading account held with OrganizationCode or to a Designated 
							Bank, or a withdrawal as per your instruction.<BR>
						</span><![if !supportLists]><span lang="EN-US">6.<span style="FONT:7pt 'Times New Roman'">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
							</span></span><![endif]><span lang="EN-US">OrganizationCode shall be released 
							of all its obligations on payment of the relevant sum for the Gold Bullion.
							<o:p></o:p></span></P>
					<![if !supportEmptyParas]><![endif]><![if !supportEmptyParas]><![endif]>
					<P class="MsoNormal" style="TEXT-JUSTIFY: inter-ideograph; LINE-HEIGHT: 12pt; TEXT-ALIGN: justify; mso-line-height-rule: exactly"><span lang="EN-US">In 
							any case, should you want to proceed further with a purchase (or sale) of Gold 
							Bullion for physical deliver, you are required to:<![if 
							!supportEmptyParas]><![endif]><![if !supportLists]></span></P>
					<P class="MsoNormal" style="TEXT-JUSTIFY: inter-ideograph; LINE-HEIGHT: 12pt; TEXT-ALIGN: justify; mso-line-height-rule: exactly"><SPAN lang="EN-US"></SPAN><span lang="EN-US">1.<span style="FONT:7pt 'Times New Roman'">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
							</span></span><![endif]><span lang="EN-US">Call the following telephone number 
							OrganizationPhone and inform our dealer that you want to enter into a transaction 
							for physical delivery.<BR>
						</span><![if !supportLists]><span lang="EN-US">2.<span style="FONT:7pt 'Times New Roman'">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
							</span></span><![endif]><span lang="EN-US">Inform the dealer your account name, 
							your account number, your e-mail address (as you have provided in the Client 
							Information Statement) and whether you want to buy, or sell, Gold Bullion for 
							physical delivery.<BR>
						</span><![if !supportLists]><span lang="EN-US">3.<span style="FONT:7pt 'Times New Roman'">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
							</span></span><![endif]><span lang="EN-US">Then you will verify your request 
							and identification by sending an e-mail message for confirmation of your 
							request. You are required to send your confirming e-mail message via the e-mail 
							address that you have identified in the Client Information Statement for the 
							purpose of our dealer to verify your identity.<BR>
						</span><![if !supportLists]><span lang="EN-US">4.<span style="FONT:7pt 'Times New Roman'">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
							</span></span><![endif]><span lang="EN-US">A dealing quote will be made via the 
							e-mail and the relevant e-mail message will carry the dealing details.<BR>
						</span><![if !supportLists]><span lang="EN-US">5.<span style="FONT:7pt 'Times New Roman'">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
							</span></span><![endif]><span lang="EN-US">Dealing quotes made via the e-mail 
							will expire automatically if our dealer has not received any response from you 
							within one minute the quote was effected. OrganizationCode¡¯s <span style="mso-spacerun:
yes">&nbsp;</span>server time stamp will be the absolute reference to the time recorded.<BR>
						</span><![if !supportLists]><span lang="EN-US">6.<span style="FONT:7pt 'Times New Roman'">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
							</span></span><![endif]><span lang="EN-US">All communication for the purpose of 
							trading under this system of dealing shall be in English (orally and/or in 
							writing).<BR>
						</span><![if !supportLists]><span lang="EN-US">7.<span style="FONT:7pt 'Times New Roman'">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
							</span></span><![endif]><span lang="EN-US">The telephone system employed by 
							OrganizationCode may be recorded on tape for the purpose of security and 
							identification. The Parties agree that such recorded conversation and 
							corresponding e-mail messages shall be <i style='mso-bidi-font-style:normal'>prima-facie</i>
							evidence of the substance communicated between <span style="mso-spacerun: yes">&nbsp;</span>OrganizationCode 
							and you for the purpose of trading effected under such circumstances.<BR>
						</span><![if !supportLists]><span lang="EN-US">8.<span style="FONT:7pt 'Times New Roman'">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
							</span></span><![endif]><span lang="EN-US">For your security measure, it may 
							take some time for our dealer to carry out the identification checks before he 
							can respond by making you a dealing quote.<o:p></o:p></span></P>
					<![if !supportEmptyParas]><![endif]><![if !supportEmptyParas]><![endif]><![if 
					!supportLists]>
					<p class="MsoNormal" style='TEXT-JUSTIFY:inter-ideograph;LINE-HEIGHT:12pt;TEXT-ALIGN:justify;mso-line-height-rule:exactly'><span lang="EN-US">A.<span style="FONT:7pt 'Times New Roman'">&nbsp;&nbsp;&nbsp;&nbsp;
							</span></span><![endif]><span lang="EN-US">Yes, I want to proceed further with 
							transaction for physical delivery, click here [then a web-page pops up showing 
							the dealing telephone number (as the telephone number may change from time to 
							time and then updated) and the dealing e-mail address (as the e-mail address 
							used for this purpose may also change and then updated) will pop up, and 
							following from there will be the list of prompting points that describe the 
							complete process of client identification, the process of credit (and 
							availability of funds or Gold Bullion) checking, the process of approval for 
							entering a trade for physical delivery, and the process of making a dealing 
							quote on Gold Bullion for physical delivery].<BR>
						</span><![if !supportLists]><span lang="EN-US">B.<span style="FONT:7pt 'Times New Roman'">&nbsp;&nbsp;&nbsp;&nbsp;
							</span></span><![endif]><span lang="EN-US">No, I want to exit, click here [EXIT 
							from trade request for physical delivery ¨C and then no further process for 
							enabling a trade for physical delivery ¨C then Client may elect to trade in 
							other contracts that are then available from the E-Trading System or continue 
							to browse the website].<BR>
						</span><![if !supportLists]><span lang="EN-US">C.<span style="FONT:7pt 'Times New Roman'">&nbsp;&nbsp;&nbsp;&nbsp;
							</span></span><![endif]><span lang="EN-US">I do not want trade Gold Bullion for 
							physical delivery but I wish to know more about trading in Gold Bullion, click 
							here [Back to E-Trading System Homepage where the Client may seek further 
							information by having a review of the Gold Bullion Trading Term Sheet (in PDF 
							form) about the terms of trading in Gold Bullion].</span><![if 
						!supportEmptyParas]><![endif]><![if !supportEmptyParas]><![endif]><![if 
						!supportEmptyParas]><![endif]><![if !supportEmptyParas]><![endif]><![if 
						!supportEmptyParas]><![endif]><![if !supportEmptyParas]><![endif]><![if 
						!supportEmptyParas]><![endif]><![if !supportEmptyParas]><![endif]><![if 
						!supportEmptyParas]><![endif]><![if !supportEmptyParas]><![endif]><o:p></o:p></p>
					<p class="MsoNormal" style='TEXT-JUSTIFY:inter-ideograph;MARGIN-LEFT:18pt;LINE-HEIGHT:12pt;TEXT-ALIGN:justify;mso-line-height-rule:exactly'><span lang="EN-US"><![if 
							!supportEmptyParas]><![endif]><FONT face="system"></FONT><o:p></o:p></span></p>
				</TD>
				<TD>
					<P>&nbsp;</P>
					<P>&nbsp;</P>
				</TD>
			</TR>
			<TR>
				<TD></TD>
				<TD>
					<P align="center"><SPAN lang="EN-US"> <SPAN style="mso-spacerun: yes">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
							</SPAN><SPAN style="mso-spacerun: yes">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
							</SPAN><SPAN style="mso-spacerun: yes">&nbsp;&nbsp; </SPAN><SPAN style="mso-spacerun: yes">
								&nbsp;</SPAN></SPAN></P>
				</TD>
				<TD></TD>
			</TR>
			<TR>
				<TD style="HEIGHT: 26px"></TD>
				<TD style="HEIGHT: 26px">
					<P align="center"><INPUT id="AgreeRadioButton" onfocus="AgreeRadioButton_OnFocus();" accessKey="a" type="radio"
							CHECKED name="AgreeRadioButton"><FONT face="system">I agree </FONT><INPUT id="DisagreeRadioButton" onfocus="DisagreeRadioButton_OnFocus();" type="radio" name="DisagreeRadioButton"><FONT face="system">I 
							disagree </FONT><INPUT id="NextButton" style="WIDTH: 79px; HEIGHT: 24px" accessKey="n" type="button" value="Next"
							onclick="javascript:NextButton_OnClick();" name="NextButton"><FONT face="system">&nbsp;<INPUT id="PrintButton" style="WIDTH: 73px; HEIGHT: 24px" accessKey="p" type="button" value="Print"
								onclick="javascript:window.print();" name="PrintButton">&nbsp;</FONT><INPUT id="ExitButton" style="WIDTH: 73px; HEIGHT: 24px" accessKey="e" type="button" value="Exit"
							onclick="javascript:window.close();" name="ExitButton"></P>
				</TD>
				<TD style="HEIGHT: 26px"></TD>
			</TR>
			<TR>
				<TD style="WIDTH: 11px"></TD>
				<TD>&nbsp;</TD>
				<TD></TD>
			</TR>
			<TR>
				<TD style="WIDTH: 11px"></TD>
				<TD></TD>
				<TD></TD>
			</TR>
		</TABLE>
	</body>
</HTML>
