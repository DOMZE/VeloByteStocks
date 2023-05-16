IF OBJECT_ID(N'dbo.Tickers', N'U') IS NULL
BEGIN
	CREATE TABLE dbo.Tickers (
		[Id] INT NOT NULL IDENTITY(1,1),
		[Ticker] VARCHAR(10) NOT NULL,
		[LastModifiedDate] DATETIMEOFFSET NOT NULL DEFAULT(GETUTCDATE()),
		CONSTRAINT PK_TickersId PRIMARY KEY NONCLUSTERED (Id)
	);
	
	CREATE INDEX IX_Ticker ON dbo.Tickers ([Ticker]);
END

IF OBJECT_ID(N'dbo.TickersHistory', N'U') IS NULL
BEGIN
	CREATE TABLE dbo.TickersHistory (
		[Id] BIGINT NOT NULL IDENTITY (1,1),
		[TickerId] INT NOT NULL,
		[Date] DATE NOT NULL,
		[Open] FLOAT NOT NULL,
		[Close] FLOAT NOT NULL,
		[High] FLOAT NOT NULL,
		[Low] FLOAT NOT NULL,
		[Volume] FLOAT NOT NULL,
		[Dividends] FLOAT NOT NULL,
		[StockSplits] FLOAT NOT NULL,
		[LastModifiedDate] DATETIMEOFFSET NOT NULL DEFAULT(GETUTCDATE()),
		CONSTRAINT PK_TickesHistoryId PRIMARY KEY NONCLUSTERED (Id),
		CONSTRAINT FK_Tickers_TickersHistory FOREIGN KEY (TickerId) REFERENCES dbo.Tickers (Id)
	);
	CREATE INDEX IX_FK_TickersId ON dbo.TickersHistory ([TickerId]);
END


