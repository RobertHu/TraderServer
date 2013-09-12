function AlignmentGrid(isGetDefault,grid,fColKey,fColAlignment,fFixedAlignment,fColWidth,fBackColorFixed,fForeColorFixed,fGridLinesFixed,fCellFontName)
{
	with (grid)
	{			
		var lngCol = ColIndex(fColKey);
		if (lngCol == -1) return;
		
		if (isGetDefault) 
		{
			ColWidth(lngCol) = fColWidth;
			//if (typeof(fCellFontName) != 'undefined') CellFontName = fCellFontName;
		}
		
		ColAlignment(lngCol) = fColAlignment;
		FixedAlignment(lngCol) = fFixedAlignment;
		FillStyle = flexFillRepeat;
		if (typeof(fBackColorFixed) != 'undefined') BackColorFixed = fBackColorFixed;
		if (typeof(fForeColorFixed) != 'undefined')	ForeColorFixed = fForeColorFixed;
		if (typeof(fGridLinesFixed) != 'undefined')	GridLinesFixed = fGridLinesFixed;
	}
}

function FontSet(grid,fFontName,fFontSize,fFontBold)
{
	grid.FontName = fFontName;
	grid.FontSize = fFontSize;
	grid.FontBold = fFontBold;
}

function MultiAccountGridInitialize(tradeConsole,grid,gridColKey,isGetDefault)
{	
	AlignmentGrid(isGetDefault,grid,gridColKey.ID,flexAlignCenterCenter,flexAlignCenterCenter,1200,10386532,0,0,"TAHOMA");
	AlignmentGrid(isGetDefault,grid,gridColKey.Code,flexAlignLeftCenter,flexAlignCenterCenter,1200);		
	AlignmentGrid(isGetDefault,grid,gridColKey.Select,flexAlignLeftCenter,flexAlignCenterCenter,300);						
	
	if (isGetDefault) FontSet(grid,"System",10,false);
	
	with (grid)
	{
		ColHidden(0) = true;
		RowHeight(0) = 420;
		ExplorerBar = flexExSortAndMove;
		var COLORSET = tradeConsole.ColorSet;
		BackColorFixed = COLORSET.AccountGridBackColorFixed;
		BackColor = COLORSET.AccountGridBackColor;
		OutlineBar = flexOutlineBarSimpleLeaf;	
		TreeColor = 255;	
	}
}

function SingleAccountDispGridInitialize(tradeConsole,grid,gridColKey,isGetDefault)
{	
	if (isGetDefault) FontSet(grid,"System",10,false);
	
	with (grid)
	{
		ColAlignment(0) = flexAlignLeftTop;
		ExplorerBar = flexExSortAndMove;
		ColWidth(0) = 1900;
		MergeCells = flexMergeFixedOnly;
		TextMatrix(0,0) = tradeConsole.Languages.AccountStatusPrompt;
		TextMatrix(0,1) = tradeConsole.Languages.AccountStatusPrompt;
		ForeColorFixed = 0;
		FixedAlignment(0) = flexAlignCenterCenter;
		FixedAlignment(1) = flexAlignCenterCenter;
		MergeRow(0) = true;
		RowHeight(0) = 420;
		
		var COLORSET = tradeConsole.ColorSet;
		BackColorFixed = COLORSET.AccountGridBackColorFixed;
		BackColor = COLORSET.AccountGridBackColor;
		OutlineBar = flexOutlineBarSimpleLeaf;	
		TreeColor = 255;	
	}
}	
		
function InstrumentGridInitialize(tradeConsole,grid,gridColKey,isGetDefault)
{
	var screenWidth = 800;	
	var defaultUI = tradeConsole.DefaultUI;
	if (defaultUI) screenWidth = defaultUI.screen.width;
	
	AlignmentGrid(isGetDefault,grid,gridColKey.Timestamp,flexAlignRightCenter,flexAlignCenterCenter,screenWidth*2,10386532,0,0,"TAHOMA");
	AlignmentGrid(isGetDefault,grid,gridColKey.Bid,flexAlignRightCenter,flexAlignRightCenter,screenWidth*1.5);				
	AlignmentGrid(isGetDefault,grid,gridColKey.Ask,flexAlignRightCenter,flexAlignRightCenter,screenWidth*1.2);
	AlignmentGrid(isGetDefault,grid,gridColKey.High,flexAlignRightCenter,flexAlignRightCenter,screenWidth*1.2);
	AlignmentGrid(isGetDefault,grid,gridColKey.Low,flexAlignRightCenter,flexAlignRightCenter,screenWidth*1.2);
	AlignmentGrid(isGetDefault,grid,gridColKey.Last,flexAlignRightCenter,flexAlignRightCenter,screenWidth*1.5);
	AlignmentGrid(isGetDefault,grid,gridColKey.Open,flexAlignRightCenter,flexAlignRightCenter,screenWidth*1.2);
	AlignmentGrid(isGetDefault,grid,gridColKey.PrevClose,flexAlignRightCenter,flexAlignRightCenter,screenWidth*1.5);
	AlignmentGrid(isGetDefault,grid,gridColKey.Change,flexAlignRightCenter,flexAlignRightCenter,screenWidth*1);
	AlignmentGrid(isGetDefault,grid,gridColKey.Select,flexAlignRightCenter,flexAlignCenterCenter,screenWidth*0.5);	
	
	if (isGetDefault) FontSet(grid,"System",12,false);
	
	with (grid)
	{
		ColHidden(0) = true;
		
		RowHeight(0) = 420;
		GridLinesFixed = 4;
		MergeCells = flexMergeFree;
		MergeRow(0) = true;
		ExtendLastCol = true;
		GridLines = 0;
		AllowUserFreezing = flexFreezeBoth;
		AllowUserResizing = flexResizeBoth;
		ExplorerBar = flexExSortAndMove;
		SelectionMod = flexSelectionFree;
		var COLORSET = tradeConsole.ColorSet;
		BackColorFixed = COLORSET.InstrumentGridBackColorFixed;	
		BackColor = COLORSET.InstrumentGridBackColor;	
	}
}

function OrderGridInitialize(tradeConsole,grid,gridColKey,isGetDefault)
{			
	AlignmentGrid(isGetDefault,grid,gridColKey.ID,flexAlignLeftCenter,flexAlignCenterCenter,1200,10386532,0,0,"TAHOMA");
	AlignmentGrid(isGetDefault,grid,gridColKey.Phase,flexAlignLeftCenter,flexAlignLeftCenter,1700);
	AlignmentGrid(isGetDefault,grid,gridColKey.AccountCode,flexAlignLeftCenter,flexAlignLeftCenter,1100);
	AlignmentGrid(isGetDefault,grid,gridColKey.InstrumentCode,flexAlignLeftCenter,flexAlignLeftCenter,1100);
	AlignmentGrid(isGetDefault,grid,gridColKey.SubmitTime,flexAlignLeftCenter,flexAlignLeftCenter,1200);
	AlignmentGrid(isGetDefault,grid,gridColKey.Lot,flexAlignRightCenter,flexAlignRightCenter,800);
	AlignmentGrid(isGetDefault,grid,gridColKey.IsOpen,flexAlignRightCenter,flexAlignRightCenter,800);
	AlignmentGrid(isGetDefault,grid,gridColKey.IsBuy,flexAlignRightCenter,flexAlignRightCenter,800);
	AlignmentGrid(isGetDefault,grid,gridColKey.SetPrice,flexAlignRightCenter,flexAlignRightCenter,1000);
	AlignmentGrid(isGetDefault,grid,gridColKey.OrderType,flexAlignLeftCenter,flexAlignLeftCenter,800);
	AlignmentGrid(isGetDefault,grid,gridColKey.CommissionSum,flexAlignRightCenter,flexAlignRightCenter,1000);
	AlignmentGrid(isGetDefault,grid,gridColKey.LevySum,flexAlignRightCenter,flexAlignRightCenter,800);
	
	if (isGetDefault) FontSet(grid,"System",10,false);
	
	with (grid)
	{				
		RowHeight(0) = 420;
		var COLORSET = tradeConsole.ColorSet;
		BackColorFixed = COLORSET.OrderGridBackColorFixed;
		BackColor = COLORSET.OrderGridBackColor;
	}	
}

function OpenOrderGridInitialize(tradeConsole,grid,gridColKey,isGetDefault)
{		
	AlignmentGrid(isGetDefault,grid,gridColKey.ID,flexAlignLeftCenter,flexAlignCenterCenter,1200,10386532,0,0,"TAHOMA");
	AlignmentGrid(isGetDefault,grid,gridColKey.IsOpen,flexAlignLeftCenter,flexAlignLeftCenter,800);
	AlignmentGrid(isGetDefault,grid,gridColKey.AccountCode,flexAlignLeftCenter,flexAlignLeftCenter,900);
	AlignmentGrid(isGetDefault,grid,gridColKey.InstrumentCode,flexAlignLeftCenter,flexAlignLeftCenter,1100);
	AlignmentGrid(isGetDefault,grid,gridColKey.ExecuteTime,flexAlignLeftCenter,flexAlignLeftCenter,1550);				
	AlignmentGrid(isGetDefault,grid,gridColKey.LotBalance,flexAlignRightCenter,flexAlignRightCenter,600);
	AlignmentGrid(isGetDefault,grid,gridColKey.IsBuy,flexAlignLeftCenter,flexAlignLeftCenter,600);
	AlignmentGrid(isGetDefault,grid,gridColKey.ExecutePrice,flexAlignRightCenter,flexAlignRightCenter,1000);
	AlignmentGrid(isGetDefault,grid,gridColKey.LivePrice,flexAlignRightCenter,flexAlignRightCenter,1000);
	AlignmentGrid(isGetDefault,grid,gridColKey.TradePLFloat,flexAlignRightCenter,flexAlignRightCenter,1400);
	AlignmentGrid(isGetDefault,grid,gridColKey.InterestPLFloat,flexAlignRightCenter,flexAlignRightCenter,1200);
	AlignmentGrid(isGetDefault,grid,gridColKey.StoragePLFloat,flexAlignRightCenter,flexAlignRightCenter,1200);				
	AlignmentGrid(isGetDefault,grid,gridColKey.Commission,flexAlignRightCenter,flexAlignRightCenter,1200);	

	if (isGetDefault) FontSet(grid,"System",10,false);
	
	with (grid)
	{
		RowHeight(0) = 420;	
		var COLORSET = tradeConsole.ColorSet;
		BackColorFixed = COLORSET.OrderGridBackColorFixed;
		BackColor = COLORSET.OpenOrderGridBackColor;
	}
}

function OrderPlacementGridInitialize(tradeConsole,grid,gridColKey,isGetDefault)
{	
	AlignmentGrid(isGetDefault,grid,gridColKey.ID,flexAlignLeftCenter,flexAlignCenterCenter,1200,10386532,0,0,"TAHOMA");
	AlignmentGrid(isGetDefault,grid,gridColKey.AccountCode,flexAlignLeftCenter,flexAlignLeftCenter,1200);
	AlignmentGrid(isGetDefault,grid,gridColKey.Lot,flexAlignLeftCenter,flexAlignLeftCenter,600);
	AlignmentGrid(isGetDefault,grid,gridColKey.IsBuy,flexAlignLeftCenter,flexAlignLeftCenter,700);
	AlignmentGrid(isGetDefault,grid,gridColKey.IsOpen,flexAlignLeftCenter,flexAlignLeftCenter,1200);
	AlignmentGrid(isGetDefault,grid,gridColKey.SetPrice,flexAlignLeftCenter,flexAlignLeftCenter,900);
	AlignmentGrid(isGetDefault,grid,gridColKey.TradeOption,flexAlignLeftCenter,flexAlignLeftCenter,900);
	
	if (isGetDefault) FontSet(grid,"System",10,false);
	
	with (grid)
	{
		if (ColIndex(gridColKey.PeerOrderCodes) != -1) ColHidden(ColIndex(gridColKey.PeerOrderCodes)) = true;
		if (ColIndex(tradeConsole.SequenceFlag) != -1) ColHidden(ColIndex(tradeConsole.SequenceFlag)) = true;
		OutlineBar = flexOutlineBarSimpleLeaf;
		var COLORSET = tradeConsole.ColorSet;
		BackColorFixed = COLORSET.DefaultGridBackColorFixed;
		BackColor = COLORSET.DefaultGridBackColor;
	}
}

function OrderOperateLiquidationGridInitialize(tradeConsole,grid,gridColKey,isGetDefault)
{	
	AlignmentGrid(isGetDefault,grid,gridColKey.ID,flexAlignLeftCenter,flexAlignCenterCenter,100,10386532,0,0,"TAHOMA");
	AlignmentGrid(isGetDefault,grid,gridColKey.Code,flexAlignLeftCenter,flexAlignCenterCenter,1200);
	AlignmentGrid(isGetDefault,grid,gridColKey.CurrencyCode,flexAlignCenterCenter,flexAlignCenterCenter,0);
	AlignmentGrid(isGetDefault,grid,gridColKey.IsBuy,flexAlignCenterCenter,flexAlignCenterCenter,600);
	AlignmentGrid(isGetDefault,grid,gridColKey.SetPrice,flexAlignRightCenter,flexAlignCenterCenter,1000);
	AlignmentGrid(isGetDefault,grid,gridColKey.LotBalance,flexAlignRightCenter,flexAlignCenterCenter,0);	
	AlignmentGrid(isGetDefault,grid,gridColKey.LiqLot,flexAlignRightCenter,flexAlignCenterCenter,600);	
	AlignmentGrid(isGetDefault,grid,gridColKey.PeerOrderCodes,flexAlignLeftCenter,flexAlignCenterCenter,2500);							

	if (isGetDefault) FontSet(grid,"System",10,false);
	
	with (grid)
	{
		var COLORSET = tradeConsole.ColorSet;
		BackColorFixed = COLORSET.DefaultGridBackColorFixed;	
		BackColor = COLORSET.DefaultGridBackColor;	
	}	
}

function OrderOperateAccountGridInitialize(tradeConsole,grid,gridColKey,isGetDefault)
{	
	AlignmentGrid(isGetDefault,grid,gridColKey.ID,flexAlignLeftCenter,flexAlignCenterCenter,1200,10386532,0,0,"TAHOMA");
	AlignmentGrid(isGetDefault,grid,gridColKey.Code,flexAlignLeftCenter,flexAlignCenterCenter,1500);
	AlignmentGrid(isGetDefault,grid,gridColKey.IsBuy,flexAlignLeftCenter,flexAlignCenterCenter,600);
	AlignmentGrid(isGetDefault,grid,gridColKey.SetPrice,flexAlignLeftCenter,flexAlignCenterCenter,900);
	AlignmentGrid(isGetDefault,grid,gridColKey.Lot,flexAlignLeftCenter,flexAlignCenterCenter,400);
	AlignmentGrid(isGetDefault,grid,gridColKey.DQMaxMove,flexAlignLeftCenter,flexAlignCenterCenter,400);
	
	if (isGetDefault) FontSet(grid,"System",10,false);
	
	with (grid)
	{
		var COLORSET = tradeConsole.ColorSet;
		BackColorFixed = COLORSET.DefaultGridBackColorFixed;
		BackColor = COLORSET.DefaultGridBackColor;
	}
}

function OutstandingOrderGridInitialize(tradeConsole,grid,gridColKey,isGetDefault)
{	
	AlignmentGrid(isGetDefault,grid,gridColKey.ID,flexAlignLeftCenter,flexAlignCenterCenter,1200,10386532,0,0,"TAHOMA");
	AlignmentGrid(isGetDefault,grid,gridColKey.TradeCode,flexAlignLeftCenter,flexAlignLeftCenter,1200);
	AlignmentGrid(isGetDefault,grid,gridColKey.ExecutePrice,flexAlignLeftCenter,flexAlignCenterCenter,900);
	AlignmentGrid(isGetDefault,grid,gridColKey.LotBalance,flexAlignLeftCenter,flexAlignCenterCenter,600);
	AlignmentGrid(isGetDefault,grid,gridColKey.OpenOrder,flexAlignLeftCenter,flexAlignCenterCenter,2800);	
	AlignmentGrid(isGetDefault,grid,gridColKey.LiqLot,flexAlignLeftCenter,flexAlignCenterCenter,500);						
	
	if (isGetDefault) FontSet(grid,"System",10,false);
	
	with (grid)
	{
		var COLORSET = tradeConsole.ColorSet;
		BackColorFixed = COLORSET.DefaultGridBackColorFixed;
		BackColor = COLORSET.DefaultGridBackColor;
	}
}

function InstrumentSelectGridInitialize(tradeConsole,grid,gridColKey,isGetDefault)
{
	AlignmentGrid(isGetDefault,grid,gridColKey.ID,flexAlignLeftCenter,flexAlignCenterCenter,1200,10386532,0,0,"TAHOMA");
	AlignmentGrid(isGetDefault,grid,gridColKey.Select,flexAlignCenterCenter,flexAlignCenterCenter,1200);				
	AlignmentGrid(isGetDefault,grid,gridColKey.Code,flexAlignCenterCenter,flexAlignCenterCenter,1200);	
	
	if (isGetDefault) FontSet(grid,"System",10,false);
	
	with (grid)
	{
		var COLORSET = tradeConsole.ColorSet;
		BackColorFixed = COLORSET.DefaultGridBackColorFixed;	
		BackColor = COLORSET.DefaultGridBackColor;					
	}
}

function AccountLockGridInitialize(tradeConsole,grid,gridColKey,isGetDefault)
{	
	AlignmentGrid(isGetDefault,grid,gridColKey.ID,flexAlignLeftCenter,flexAlignCenterCenter,1200,10386532,0,0,"TAHOMA");
	AlignmentGrid(isGetDefault,grid,gridColKey.Select,flexAlignLeftCenter,flexAlignLeftCenter,1000);				
	AlignmentGrid(isGetDefault,grid,gridColKey.Code,flexAlignLeftCenter,flexAlignLeftCenter,1300);	
					
	if (isGetDefault) FontSet(grid,"System",10,false);
	
	with (grid)
	{
		var COLORSET = tradeConsole.ColorSet;
		BackColorFixed = COLORSET.DefaultGridBackColorFixed;	
		BackColor= COLORSET.DefaultGridBackColor;	
	}				
}
