IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DIM_CUSTOMER]') AND type in (N'U')) DROP TABLE [dbo].[DIM_CUSTOMER]

CREATE TABLE [dbo].[DIM_CUSTOMER](
	[DIM_CUSTOMER_SK] [int] IDENTITY(1,1) NOT NULL,
	[INSERT_MODULE_INSTANCE_ID] [int] NOT NULL,
	[RECORD_CHECKSUM_TYPE1] [char](32) NOT NULL,
	[RECORD_CHECKSUM_TYPE2] [char](32) NOT NULL,
	[CHANGE_DATETIME] [datetime2](7) NOT NULL,
	[CHANGE_EXPIRY_DATETIME] [datetime2](7) NOT NULL,
	[CHANGE_DATA_INDICATOR] [varchar](1) NOT NULL,
	[CUSTOMER_SK] [binary](16) NOT NULL,
	[CUSTOMER_ID] [nvarchar](100) NOT NULL,
	[GIVEN_NAME] [nvarchar](100) NOT NULL,
	[SURNAME] [nvarchar](100) NOT NULL,
	[PREF_GENDER_PRONOUN] [nvarchar](100) NOT NULL,
	[SUBURB] [nvarchar](100) NOT NULL,
	[POSTCODE] [nvarchar](100) NOT NULL,
	[COUNTRY] [nvarchar](100) NOT NULL,
	[DATE_OF_BIRTH] [datetime2](7) NOT NULL,
	[CONTACT_NUMBER] [nvarchar](100) NULL,
	[STATE] [nvarchar](100) NULL,
 CONSTRAINT [PK_DIM_CUSTOMER_TBL] PRIMARY KEY CLUSTERED 
(
	[DIM_CUSTOMER_SK] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
