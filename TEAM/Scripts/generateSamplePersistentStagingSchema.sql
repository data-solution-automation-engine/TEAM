IF OBJECT_ID('dbo.PSA_PROFILER_CUST_MEMBERSHIP', 'U') IS NOT NULL DROP TABLE[dbo].[PSA_PROFILER_CUST_MEMBERSHIP]
IF OBJECT_ID('dbo.PSA_PROFILER_CUSTOMER_OFFER', 'U') IS NOT NULL DROP TABLE[dbo].[PSA_PROFILER_CUSTOMER_OFFER]
IF OBJECT_ID('dbo.PSA_PROFILER_CUSTOMER_PERSONAL', 'U') IS NOT NULL DROP TABLE[dbo].[PSA_PROFILER_CUSTOMER_PERSONAL]
IF OBJECT_ID('dbo.PSA_PROFILER_ESTIMATED_WORTH', 'U') IS NOT NULL DROP TABLE[dbo].[PSA_PROFILER_ESTIMATED_WORTH]
IF OBJECT_ID('dbo.PSA_PROFILER_OFFER', 'U') IS NOT NULL DROP TABLE[dbo].[PSA_PROFILER_OFFER]
IF OBJECT_ID('dbo.PSA_PROFILER_PERSONALISED_COSTING', 'U') IS NOT NULL DROP TABLE[dbo].[PSA_PROFILER_PERSONALISED_COSTING]
IF OBJECT_ID('dbo.PSA_PROFILER_PLAN', 'U') IS NOT NULL DROP TABLE[dbo].[PSA_PROFILER_PLAN]
IF OBJECT_ID('dbo.PSA_USERMANAGED_SEGMENT', 'U') IS NOT NULL DROP TABLE[dbo].[PSA_USERMANAGED_SEGMENT]

CREATE TABLE [PSA_PROFILER_CUST_MEMBERSHIP]
(
  [INSCRIPTION_TIMESTAMP] datetime2(7) NOT NULL,
  [INSCRIPTION_RECORD_ID] integer NOT NULL,
  [SOURCE_TIMESTAMP] datetime2(7) NOT NULL,
  [CHANGE_DATA_INDICATOR] varchar(100) NOT NULL,
  [AUDIT_TRAIL_ID] integer NOT NULL,
  [CHECKSUM] binary(16) NOT NULL,
  [CustomerID] integer NOT NULL,
  [Plan_Code] nvarchar(100) NOT NULL,
  [Start_Date] datetime2(7) NULL,
  [End_Date] datetime2(7) NULL,
  [Status] nvarchar(100) NULL,
  [Comment] nvarchar(100) NULL
  CONSTRAINT [PK_PSA_PROFILER_CUST_MEMBERSHIP] PRIMARY KEY NONCLUSTERED ([CustomerID] ASC, [Plan_Code] ASC, [INSCRIPTION_TIMESTAMP] ASC, [INSCRIPTION_RECORD_ID] ASC
)
)

CREATE TABLE [PSA_PROFILER_CUSTOMER_OFFER]
(
  [INSCRIPTION_TIMESTAMP] datetime2(7) NOT NULL,
  [INSCRIPTION_RECORD_ID] integer NOT NULL,
  [SOURCE_TIMESTAMP] datetime2(7) NOT NULL,
  [CHANGE_DATA_INDICATOR] varchar(100) NOT NULL,
  [AUDIT_TRAIL_ID] integer NOT NULL,
  [CHECKSUM] binary(16) NOT NULL,
  [CustomerID] integer NOT NULL,
  [OfferID] integer NOT NULL,
  CONSTRAINT [PK_PSA_PROFILER_CUSTOMER_OFFER] PRIMARY KEY NONCLUSTERED ([CustomerID] ASC, [OfferID] ASC, [INSCRIPTION_TIMESTAMP] ASC, [INSCRIPTION_RECORD_ID] ASC
)
)

CREATE TABLE [PSA_PROFILER_CUSTOMER_PERSONAL]
(
  [INSCRIPTION_TIMESTAMP] datetime2(7) NOT NULL,
  [INSCRIPTION_RECORD_ID] integer NOT NULL,
  [SOURCE_TIMESTAMP] datetime2(7) NOT NULL,
  [CHANGE_DATA_INDICATOR] varchar(100) NOT NULL,
  [AUDIT_TRAIL_ID] integer NOT NULL,
  [CHECKSUM] binary(16) NOT NULL,
  [CustomerID] integer NOT NULL,
  [Given] nvarchar(100) NULL,
  [Surname] nvarchar(100) NULL,
  [Suburb] nvarchar(100) NULL,
  [State] nvarchar(100) NULL,
  [Postcode] nvarchar(100) NULL,
  [Country] nvarchar(100) NULL,
  [Gender] nvarchar(100) NULL,
  [DOB] datetime2(7) NULL,
  [Contact_Number] integer NULL,
  [Referee_Offer_Made] integer NULL,
  CONSTRAINT [PK_PSA_PROFILER_CUSTOMER_PERSONAL] PRIMARY KEY NONCLUSTERED([CustomerID] ASC, [INSCRIPTION_TIMESTAMP] ASC, [INSCRIPTION_RECORD_ID] ASC
)
)

CREATE TABLE [PSA_PROFILER_ESTIMATED_WORTH]
(
  [INSCRIPTION_TIMESTAMP] datetime2(7) NOT NULL,
  [INSCRIPTION_RECORD_ID] integer NOT NULL,
  [SOURCE_TIMESTAMP] datetime2(7) NOT NULL,
  [CHANGE_DATA_INDICATOR] varchar(100) NOT NULL,
  [AUDIT_TRAIL_ID] integer NOT NULL,
  [CHECKSUM] binary(16) NOT NULL,
  [Plan_Code] nvarchar(100) NOT NULL,
  [Date_effective] datetime2(7) NOT NULL,
  [Value_Amount] numeric(38,20) NULL,
  CONSTRAINT [PK_PSA_PROFILER_ESTIMATED_WORTH] PRIMARY KEY NONCLUSTERED([Plan_Code] ASC, [Date_effective] ASC, [INSCRIPTION_TIMESTAMP] ASC, [INSCRIPTION_RECORD_ID] ASC
)
)

CREATE TABLE [PSA_PROFILER_OFFER]
(
  [INSCRIPTION_TIMESTAMP] datetime2(7) NOT NULL,
  [INSCRIPTION_RECORD_ID] integer NOT NULL,
  [SOURCE_TIMESTAMP] datetime2(7) NOT NULL,
  [CHANGE_DATA_INDICATOR] varchar(100) NOT NULL,
  [AUDIT_TRAIL_ID] integer NOT NULL,
  [CHECKSUM] binary(16) NOT NULL,
  [OfferID] integer NOT NULL,
  [Offer_Long_Description] nvarchar(100) NULL,
  CONSTRAINT [PK_PSA_PROFILER_OFFER] PRIMARY KEY NONCLUSTERED([OfferID] ASC, [INSCRIPTION_TIMESTAMP] ASC, [INSCRIPTION_RECORD_ID] ASC
)
)

CREATE TABLE [PSA_PROFILER_PERSONALISED_COSTING]
(
  [INSCRIPTION_TIMESTAMP] datetime2(7) NOT NULL,
  [INSCRIPTION_RECORD_ID] integer NOT NULL,
  [SOURCE_TIMESTAMP] datetime2(7) NOT NULL,
  [CHANGE_DATA_INDICATOR] varchar(100) NOT NULL,
  [AUDIT_TRAIL_ID] integer NOT NULL,
  [CHECKSUM] binary(16) NOT NULL,
  [Member] integer NOT NULL,
  [Segment] nvarchar(100) NOT NULL,
  [Plan_Code] nvarchar(100) NOT NULL,
  [Date_effective] datetime2(7) NOT NULL,
  [Monthly_Cost] numeric(38,20) NULL,
  CONSTRAINT [PK_PSA_PROFILER_PERSONALISED_COSTING] PRIMARY KEY NONCLUSTERED([Member] ASC, [Segment] ASC, [Plan_Code] ASC, [Date_effective] ASC, [INSCRIPTION_TIMESTAMP] ASC, [INSCRIPTION_RECORD_ID] ASC
)
)

CREATE TABLE [PSA_PROFILER_PLAN]
(
  [INSCRIPTION_TIMESTAMP] datetime2(7) NOT NULL,
  [INSCRIPTION_RECORD_ID] integer NOT NULL,
  [SOURCE_TIMESTAMP] datetime2(7) NOT NULL,
  [CHANGE_DATA_INDICATOR] varchar(100) NOT NULL,
  [AUDIT_TRAIL_ID] integer NOT NULL,
  [CHECKSUM] binary(16) NOT NULL,
  [Plan_Code] nvarchar(100) NOT NULL,
  [Plan_Desc] nvarchar(100) NULL,
  [Renewal_Plan_Code] nvarchar(100) NULL
  CONSTRAINT [PK_PSA_PROFILER_PLAN] PRIMARY KEY NONCLUSTERED([Plan_Code] ASC, [INSCRIPTION_TIMESTAMP] ASC, [INSCRIPTION_RECORD_ID] ASC
)
)

CREATE TABLE [PSA_USERMANAGED_SEGMENT]
(
  [INSCRIPTION_TIMESTAMP] datetime2(7) NOT NULL,
  [INSCRIPTION_RECORD_ID] integer NOT NULL,
  [SOURCE_TIMESTAMP] datetime2(7) NOT NULL,
  [CHANGE_DATA_INDICATOR] varchar(100) NOT NULL,
  [AUDIT_TRAIL_ID] integer NOT NULL,
  [CHECKSUM] binary(16) NOT NULL,
  [Demographic_Segment_Code] nvarchar(100) NOT NULL,
  [Demographic_Segment_Description] nvarchar(100) NULL,
  CONSTRAINT [PK_PSA_USERMANAGED_SEGMENT] PRIMARY KEY CLUSTERED([Demographic_Segment_Code] ASC, [INSCRIPTION_TIMESTAMP] ASC, [INSCRIPTION_RECORD_ID] ASC
)
)