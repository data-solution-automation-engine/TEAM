IF OBJECT_ID('dbo.STG_PROFILER_CUST_MEMBERSHIP', 'U') IS NOT NULL DROP TABLE [dbo].[STG_PROFILER_CUST_MEMBERSHIP]
IF OBJECT_ID('dbo.STG_PROFILER_CUSTOMER_OFFER', 'U') IS NOT NULL DROP TABLE [dbo].[STG_PROFILER_CUSTOMER_OFFER]
IF OBJECT_ID('dbo.STG_PROFILER_CUSTOMER_PERSONAL', 'U') IS NOT NULL DROP TABLE [dbo].[STG_PROFILER_CUSTOMER_PERSONAL]
IF OBJECT_ID('dbo.STG_PROFILER_ESTIMATED_WORTH', 'U') IS NOT NULL DROP TABLE [dbo].[STG_PROFILER_ESTIMATED_WORTH]
IF OBJECT_ID('dbo.STG_PROFILER_OFFER', 'U') IS NOT NULL DROP TABLE [dbo].[STG_PROFILER_OFFER]
IF OBJECT_ID('dbo.STG_PROFILER_PERSONALISED_COSTING', 'U') IS NOT NULL DROP TABLE [dbo].[STG_PROFILER_PERSONALISED_COSTING]
IF OBJECT_ID('dbo.STG_PROFILER_PLAN', 'U') IS NOT NULL DROP TABLE [dbo].[STG_PROFILER_PLAN]
IF OBJECT_ID('dbo.STG_USERMANAGED_SEGMENT', 'U') IS NOT NULL DROP TABLE [dbo].[STG_USERMANAGED_SEGMENT]

/* Create the tables */
CREATE TABLE [STG_PROFILER_CUST_MEMBERSHIP]
(
  [OMD_INSERT_MODULE_INSTANCE_ID] [int] NOT NULL,
  [OMD_INSERT_DATETIME] [datetime2] (7) NOT NULL DEFAULT SYSDATETIME(),
  [OMD_EVENT_DATETIME] [datetime2] (7) NOT NULL,
  [OMD_RECORD_SOURCE] [varchar] (100) NOT NULL,
  [OMD_SOURCE_ROW_ID] [int] IDENTITY(1,1) NOT NULL,
  [OMD_CDC_OPERATION] [varchar] (100) NOT NULL,
  [OMD_HASH_FULL_RECORD] [binary] (16) NOT NULL,
  [CustomerID] int NULL,
  [Plan_Code] nvarchar(100) NULL,
  [Start_Date] datetime2(7) NULL,
  [End_Date] datetime2(7) NULL,
  [Status] nvarchar(100) NULL,
  [Comment] nvarchar(100) NULL 
)

EXEC sp_addextendedproperty
@name = 'Natural_Key', @value = 'Yes',
@level0type = 'SCHEMA', @level0name = 'dbo',
@level1type = 'TABLE', @level1name = 'STG_PROFILER_CUST_MEMBERSHIP',
@level2type = 'COLUMN', @level2name = 'CustomerID'

EXEC sp_addextendedproperty
@name = 'Natural_Key', @value = 'Yes',
@level0type = 'SCHEMA', @level0name = 'dbo',
@level1type = 'TABLE', @level1name = 'STG_PROFILER_CUST_MEMBERSHIP',
@level2type = 'COLUMN', @level2name = 'Plan_Code'

CREATE TABLE [STG_PROFILER_CUSTOMER_OFFER]
(
  [OMD_INSERT_MODULE_INSTANCE_ID] [int] NOT NULL,
  [OMD_INSERT_DATETIME] [datetime2] (7) NOT NULL DEFAULT SYSDATETIME(),
  [OMD_EVENT_DATETIME] [datetime2] (7) NOT NULL,
  [OMD_RECORD_SOURCE] [varchar] (100) NOT NULL,
  [OMD_SOURCE_ROW_ID] [int] IDENTITY(1,1) NOT NULL,
  [OMD_CDC_OPERATION] [varchar] (100) NOT NULL,
  [OMD_HASH_FULL_RECORD] [binary] (16) NOT NULL,
  [CustomerID] int NULL,
  [OfferID] int NULL 
)

EXEC sp_addextendedproperty
@name = 'Natural_Key', @value = 'Yes',
@level0type = 'SCHEMA', @level0name = 'dbo',
@level1type = 'TABLE', @level1name = 'STG_PROFILER_CUSTOMER_OFFER',
@level2type = 'COLUMN', @level2name = 'CustomerID'

EXEC sp_addextendedproperty
@name = 'Natural_Key', @value = 'Yes',
@level0type = 'SCHEMA', @level0name = 'dbo',
@level1type = 'TABLE', @level1name = 'STG_PROFILER_CUSTOMER_OFFER',
@level2type = 'COLUMN', @level2name = 'OfferID'

CREATE TABLE [STG_PROFILER_CUSTOMER_PERSONAL]
(
  [OMD_INSERT_MODULE_INSTANCE_ID] [int] NOT NULL,
  [OMD_INSERT_DATETIME] [datetime2] (7) NOT NULL DEFAULT SYSDATETIME(),
  [OMD_EVENT_DATETIME] [datetime2] (7) NOT NULL,
  [OMD_RECORD_SOURCE] [varchar] (100) NOT NULL,
  [OMD_SOURCE_ROW_ID] [int] IDENTITY(1,1) NOT NULL,
  [OMD_CDC_OPERATION] [varchar] (100) NOT NULL,
  [OMD_HASH_FULL_RECORD] [binary] (16) NOT NULL,
  [CustomerID] int NULL,
  [Given] nvarchar(100) NULL,
  [Surname] nvarchar(100) NULL,
  [Suburb] nvarchar(100) NULL,
  [State] nvarchar(100) NULL,
  [Postcode] nvarchar(100) NULL,
  [Country] nvarchar(100) NULL,
  [Gender] nvarchar(100) NULL,
  [DOB] datetime2(7) NULL,
  [Contact_Number] int NULL,
  [Referee_Offer_Made] int NULL 
)

EXEC sp_addextendedproperty
@name = 'Natural_Key', @value = 'Yes',
@level0type = 'SCHEMA', @level0name = 'dbo',
@level1type = 'TABLE', @level1name = 'STG_PROFILER_CUSTOMER_PERSONAL',
@level2type = 'COLUMN', @level2name = 'CustomerID'

CREATE TABLE [STG_PROFILER_ESTIMATED_WORTH]
(
  [OMD_INSERT_MODULE_INSTANCE_ID] [int] NOT NULL,
  [OMD_INSERT_DATETIME] [datetime2] (7) NOT NULL DEFAULT SYSDATETIME(),
  [OMD_EVENT_DATETIME] [datetime2] (7) NOT NULL,
  [OMD_RECORD_SOURCE] [varchar] (100) NOT NULL,
  [OMD_SOURCE_ROW_ID] [int] IDENTITY(1,1) NOT NULL,
  [OMD_CDC_OPERATION] [varchar] (100) NOT NULL,
  [OMD_HASH_FULL_RECORD] [binary] (16) NOT NULL,
  [Plan_Code] nvarchar(100) NULL,
  [Date_effective] datetime2(7) NULL,
  [Value_Amount] numeric(38,20) NULL 
)

EXEC sp_addextendedproperty
@name = 'Natural_Key', @value = 'Yes',
@level0type = 'SCHEMA', @level0name = 'dbo',
@level1type = 'TABLE', @level1name = 'STG_PROFILER_ESTIMATED_WORTH',
@level2type = 'COLUMN', @level2name = 'Plan_Code'

EXEC sp_addextendedproperty
@name = 'Natural_Key', @value = 'Yes',
@level0type = 'SCHEMA', @level0name = 'dbo',
@level1type = 'TABLE', @level1name = 'STG_PROFILER_ESTIMATED_WORTH',
@level2type = 'COLUMN', @level2name = 'Date_effective'

CREATE TABLE [STG_PROFILER_OFFER]
(
  [OMD_INSERT_MODULE_INSTANCE_ID] [int] NOT NULL,
  [OMD_INSERT_DATETIME] [datetime2] (7) NOT NULL DEFAULT SYSDATETIME(),
  [OMD_EVENT_DATETIME] [datetime2] (7) NOT NULL,
  [OMD_RECORD_SOURCE] [varchar] (100) NOT NULL,
  [OMD_SOURCE_ROW_ID] [int] IDENTITY(1,1) NOT NULL,
  [OMD_CDC_OPERATION] [varchar] (100) NOT NULL,
  [OMD_HASH_FULL_RECORD] [binary] (16) NOT NULL,
  [OfferID] int NULL,
  [Offer_Long_Description] nvarchar(100) NULL
)

EXEC sp_addextendedproperty
@name = 'Natural_Key', @value = 'Yes',
@level0type = 'SCHEMA', @level0name = 'dbo',
@level1type = 'TABLE', @level1name = 'STG_PROFILER_OFFER',
@level2type = 'COLUMN', @level2name = 'OfferID'

CREATE TABLE [STG_PROFILER_PERSONALISED_COSTING]
(
  [OMD_INSERT_MODULE_INSTANCE_ID] [int] NOT NULL,
  [OMD_INSERT_DATETIME] [datetime2] (7) NOT NULL DEFAULT SYSDATETIME(),
  [OMD_EVENT_DATETIME] [datetime2] (7) NOT NULL,
  [OMD_RECORD_SOURCE] [varchar] (100) NOT NULL,
  [OMD_SOURCE_ROW_ID] [int] IDENTITY(1,1) NOT NULL,
  [OMD_CDC_OPERATION] [varchar] (100) NOT NULL,
  [OMD_HASH_FULL_RECORD] [binary] (16) NOT NULL,
  [Member] int NULL,
  [Segment] nvarchar(100) NULL,
  [Plan_Code] nvarchar(100) NULL,
  [Date_effective] datetime2(7) NULL,
  [Monthly_Cost] numeric(38,20) NULL 
)

EXEC sp_addextendedproperty
@name = 'Natural_Key', @value = 'Yes',
@level0type = 'SCHEMA', @level0name = 'dbo',
@level1type = 'TABLE', @level1name = 'STG_PROFILER_PERSONALISED_COSTING',
@level2type = 'COLUMN', @level2name = 'Segment'

EXEC sp_addextendedproperty
@name = 'Natural_Key', @value = 'Yes',
@level0type = 'SCHEMA', @level0name = 'dbo',
@level1type = 'TABLE', @level1name = 'STG_PROFILER_PERSONALISED_COSTING',
@level2type = 'COLUMN', @level2name = 'Plan_Code'

EXEC sp_addextendedproperty
@name = 'Natural_Key', @value = 'Yes',
@level0type = 'SCHEMA', @level0name = 'dbo',
@level1type = 'TABLE', @level1name = 'STG_PROFILER_PERSONALISED_COSTING',
@level2type = 'COLUMN', @level2name = 'Date_effective'

CREATE TABLE [STG_PROFILER_PLAN]
(
  [OMD_INSERT_MODULE_INSTANCE_ID] [int] NOT NULL,
  [OMD_INSERT_DATETIME] [datetime2] (7) NOT NULL DEFAULT SYSDATETIME(),
  [OMD_EVENT_DATETIME] [datetime2] (7) NOT NULL,
  [OMD_RECORD_SOURCE] [varchar] (100) NOT NULL,
  [OMD_SOURCE_ROW_ID] [int] IDENTITY(1,1) NOT NULL,
  [OMD_CDC_OPERATION] [varchar] (100) NOT NULL,
  [OMD_HASH_FULL_RECORD] [binary] (16) NOT NULL,
  [Plan_Code] nvarchar(100) NULL,
  [Plan_Desc] nvarchar(100) NULL,
  [Renewal_Plan_Code] nvarchar(100) NULL
)

EXEC sp_addextendedproperty
@name = 'Natural_Key', @value = 'Yes',
@level0type = 'SCHEMA', @level0name = 'dbo',
@level1type = 'TABLE', @level1name = 'STG_PROFILER_PLAN',
@level2type = 'COLUMN', @level2name = 'Plan_Code'

CREATE TABLE [STG_USERMANAGED_SEGMENT]
(
  [OMD_INSERT_MODULE_INSTANCE_ID] [int] NOT NULL,
  [OMD_INSERT_DATETIME] [datetime2] (7) NOT NULL DEFAULT SYSDATETIME(),
  [OMD_EVENT_DATETIME] [datetime2] (7) NOT NULL,
  [OMD_RECORD_SOURCE] [varchar] (100) NOT NULL,
  [OMD_SOURCE_ROW_ID] [int] IDENTITY(1,1) NOT NULL,
  [OMD_CDC_OPERATION] [varchar] (100) NOT NULL,
  [OMD_HASH_FULL_RECORD] [binary] (16) NOT NULL,
  [Demographic_Segment_Code] nvarchar(100) NULL,
  [Demographic_Segment_Description] nvarchar(100) NULL 
)

EXEC sp_addextendedproperty
@name = 'Natural_Key', @value = 'Yes',
@level0type = 'SCHEMA', @level0name = 'dbo',
@level1type = 'TABLE', @level1name = 'STG_USERMANAGED_SEGMENT',
@level2type = 'COLUMN', @level2name = 'Demographic_Segment_Description'

/* Create the content (for the User Managed Staging table) */
INSERT INTO[dbo].[STG_USERMANAGED_SEGMENT] (
  [OMD_INSERT_MODULE_INSTANCE_ID]
 ,[OMD_INSERT_DATETIME]
 ,[OMD_EVENT_DATETIME]
 ,[OMD_RECORD_SOURCE]
 ,[OMD_CDC_OPERATION]
 ,[OMD_HASH_FULL_RECORD]
 ,[Demographic_Segment_Code]
 ,[Demographic_Segment_Description])
VALUES
 ( -1, GETDATE(), GETDATE(), 'Data Warehouse','Insert',0x00, 'LOW', 'Lower SES'),
 ( -1,GETDATE(), GETDATE(), 'Data Warehouse','Insert',0x00, 'MED', 'Medium SES'),
 ( -1,GETDATE(), GETDATE(), 'Data Warehouse','Insert',0x00, 'HIGH','High SES')