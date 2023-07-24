/* Drop the tables, if they exist */
IF OBJECT_ID('dbo.ESTIMATED_WORTH', 'U') IS NOT NULL DROP TABLE [ESTIMATED_WORTH]
IF OBJECT_ID('dbo.PERSONALISED_COSTING', 'U') IS NOT NULL DROP TABLE [PERSONALISED_COSTING]
IF OBJECT_ID('dbo.CUST_MEMBERSHIP', 'U') IS NOT NULL DROP TABLE [CUST_MEMBERSHIP]
IF OBJECT_ID('dbo.PLAN', 'U') IS NOT NULL DROP TABLE [PLAN]
IF OBJECT_ID('dbo.CUSTOMER_OFFER', 'U') IS NOT NULL DROP TABLE [CUSTOMER_OFFER]
IF OBJECT_ID('dbo.OFFER', 'U') IS NOT NULL DROP TABLE [OFFER]
IF OBJECT_ID('dbo.CUSTOMER_PERSONAL', 'U') IS NOT NULL DROP TABLE [CUSTOMER_PERSONAL]
IF OBJECT_ID('dbo.CUSTOMER_CONTACT', 'U') IS NOT NULL DROP TABLE [CUSTOMER_CONTACT]

/* Create the tables */
CREATE TABLE [CUST_MEMBERSHIP]
(
  [CustomerID] integer NOT NULL,
  [Plan_Code] varchar(100) NOT NULL,
  [Start_Date] datetime NULL,
  [End_Date] datetime NULL,
  [Status] varchar(10) NULL,
  [Comment] varchar(50) NULL,
  CONSTRAINT [PK_CUST_MEMBERSHIP] PRIMARY KEY CLUSTERED(CustomerID ASC, Plan_Code ASC)
)

CREATE TABLE [CUSTOMER_OFFER]
(
  [CustomerID] integer NOT NULL,
  [OfferID] integer NOT NULL,
  CONSTRAINT [PK_CUSTOMER_OFFER] PRIMARY KEY CLUSTERED (CustomerID ASC, OfferID ASC)
)

CREATE TABLE [CUSTOMER_PERSONAL]
(
  [CustomerID] integer NOT NULL,
  [Given] varchar(100) NULL,
  [Surname] varchar(100) NULL,
  [Suburb] varchar(50) NULL,
  [State] varchar(3) NULL,
  [Postcode] varchar(6) NULL,
  [Country] varchar(100) NULL,
  [Pronoun] varchar(1) NULL,
  [DOB] date NULL,
  [Referee_Offer_Made] integer NULL,
  [Valid_From] date NULL,
  CONSTRAINT [PK_CUSTOMER_PERSONAL] PRIMARY KEY CLUSTERED (CustomerID ASC)
)

CREATE TABLE [CUSTOMER_CONTACT]
(
  [CustomerID] integer NOT NULL,
  [Contact_Number] integer NULL,
  [Valid_From] date NULL,
  CONSTRAINT [PK_CUSTOMER_CONTACT] PRIMARY KEY CLUSTERED (CustomerID ASC)
)

CREATE TABLE [ESTIMATED_WORTH]
(
  [Plan_Code] varchar(100) NOT NULL,
  [Date_effective] datetime NOT NULL,
  [Value_Amount] numeric NULL,
  CONSTRAINT [PK_ESTIMATED_WORTH] PRIMARY KEY CLUSTERED(Plan_Code ASC, Date_effective ASC)
)

CREATE TABLE [OFFER]
(
  [OfferID] integer NOT NULL,
  [Offer_Long_Description] varchar(100) NULL,
  CONSTRAINT [PK_OFFER] PRIMARY KEY CLUSTERED(OfferID ASC)
)

CREATE TABLE [PERSONALISED_COSTING]
(
  [Member] integer NOT NULL,
  [Segment] varchar(100) NOT NULL,
  [Plan_Code] varchar(100) NOT NULL,
  [Date_effective] datetime NOT NULL,
  [Monthly_Cost] numeric NULL,
  CONSTRAINT [PK_PERSONALISED_COSTING] PRIMARY KEY CLUSTERED(Member ASC, Segment ASC, Plan_Code ASC, Date_effective ASC)
)

CREATE TABLE [PLAN]
(
  [Plan_Code] varchar(100) NOT NULL,
  [Plan_Desc]varchar(100) NULL,
  [Renewal_Plan_Code] varchar(100) NULL,
  CONSTRAINT [PK_PLAN] PRIMARY KEY CLUSTERED(Plan_Code ASC)
)

/* Create the sample content */
INSERT [dbo].[CUSTOMER_PERSONAL] ([CustomerID], [Given], [Surname], [Suburb], [State], [Postcode], [Country], [Pronoun], [DOB], [Referee_Offer_Made], [Valid_From]) VALUES(235892, N'Simon', N'Someone', N'Sydney', N'NSW', N'1000', N'Australia', N'M', CAST(N'1960-12-10' AS Date), 1, CAST(N'2023-01-01' AS Date))
INSERT [dbo].[CUSTOMER_PERSONAL] ([CustomerID], [Given], [Surname], [Suburb], [State], [Postcode], [Country], [Pronoun], [DOB], [Referee_Offer_Made], [Valid_From]) VALUES(258279, N'Jason', N'Doe', N'Indooropilly', N'QLD', N'4000', N'Australia', N'M', CAST(N'1980-01-04' AS Date), 1, CAST(N'2023-01-01' AS Date))
INSERT [dbo].[CUSTOMER_PERSONAL] ([CustomerID], [Given], [Surname], [Suburb], [State], [Postcode], [Country], [Pronoun], [DOB], [Referee_Offer_Made], [Valid_From]) VALUES(321799, N'Julie', N'Sray', N'London', N'N/A', N'0000', N'UK', N'M', CAST(N'1951-01-04' AS Date), 1, CAST(N'2023-01-01' AS Date))
INSERT [dbo].[CUSTOMER_PERSONAL] ([CustomerID], [Given], [Surname], [Suburb], [State], [Postcode], [Country], [Pronoun], [DOB], [Referee_Offer_Made], [Valid_From]) VALUES(683492, N'Mary', N'Smith', N'Bulimba', N'QLD', N'3000', N'Australia', N'F', CAST(N'1977-04-12' AS Date), 0, CAST(N'2023-01-01' AS Date))
INSERT [dbo].[CUSTOMER_PERSONAL] ([CustomerID], [Given], [Surname], [Suburb], [State], [Postcode], [Country], [Pronoun], [DOB], [Referee_Offer_Made], [Valid_From]) VALUES(885325, N'Michael', N'Evans', N'Bourke', N'NWS', N'2000', N'Australia', N'M', CAST(N'1985-04-19' AS Date), 0, CAST(N'2023-01-01' AS Date))
-- Backdated adjustment example, first row
--INSERT [dbo].[CUSTOMER_PERSONAL] ([CustomerID], [Given], [Surname], [Suburb], [State], [Postcode], [Country], [Pronoun], [DOB], [Contact_Number], [Referee_Offer_Made], [Valid_From]) VALUES(100000, N'Jonathan', NULL, N'London', N'N/A', N'0000', N'UK', N'M', CAST(N'1951-01-04' AS Date), 123, 1, CAST(N'2010-02-01' AS Date))
--
INSERT [dbo].[CUSTOMER_CONTACT] ([CustomerID], [Contact_Number], [Valid_From]) VALUES(235892, 9874634,CAST(N'2023-01-01' AS Date))
INSERT [dbo].[CUSTOMER_CONTACT] ([CustomerID], [Contact_Number], [Valid_From]) VALUES(258279, 41234, CAST(N'2023-01-01' AS Date))
INSERT [dbo].[CUSTOMER_CONTACT] ([CustomerID], [Contact_Number], [Valid_From]) VALUES(321799, 23555, CAST(N'2023-01-01' AS Date))
INSERT [dbo].[CUSTOMER_CONTACT] ([CustomerID], [Contact_Number], [Valid_From]) VALUES(683492, 41234, CAST(N'2023-01-01' AS Date))
INSERT [dbo].[CUSTOMER_CONTACT] ([CustomerID], [Contact_Number], [Valid_From]) VALUES(885325, 89235, CAST(N'2023-01-01' AS Date))

INSERT [dbo].[OFFER] ([OfferID], [Offer_Long_Description]) VALUES(450, N'20% off all future purchases')
INSERT [dbo].[OFFER] ([OfferID], [Offer_Long_Description]) VALUES(462, N'10% off all future purchases')
INSERT [dbo].[OFFER] ([OfferID], [Offer_Long_Description]) VALUES(469, N'Free movie tickets')
--
INSERT [dbo].[PLAN] ([Plan_Code], [Plan_Desc], [Renewal_Plan_Code]) VALUES(N'AVG', N'Average / Mix plan', 'SUPR')
INSERT [dbo].[PLAN] ([Plan_Code], [Plan_Desc], [Renewal_Plan_Code]) VALUES(N'HIGH', N'Highroller / risk embracing', 'SUPR')
INSERT [dbo].[PLAN] ([Plan_Code], [Plan_Desc], [Renewal_Plan_Code]) VALUES(N'LOW', N'Risk avoiding', 'MAXM')
--
INSERT [dbo].[CUST_MEMBERSHIP] ([CustomerID], [Plan_Code], [Start_Date], [End_Date], [Status], [Comment]) VALUES(235892, N'HIGH', CAST(N'2012-05-12T00:00:00.000' AS DateTime), CAST(N'2015-12-31T00:00:00.000' AS DateTime), N'High', N'Trial')
INSERT [dbo].[CUST_MEMBERSHIP] ([CustomerID], [Plan_Code], [Start_Date], [End_Date], [Status], [Comment]) VALUES(321799, N'AVG', CAST(N'2010-01-01T00:00:00.000' AS DateTime), CAST(N'2014-10-28T00:00:00.000' AS DateTime), N'Open', N'None')
INSERT [dbo].[CUST_MEMBERSHIP] ([CustomerID], [Plan_Code], [Start_Date], [End_Date], [Status], [Comment]) VALUES(683492, N'LOW', CAST(N'2012-12-12T00:00:00.000' AS DateTime), CAST(N'2020-02-27T00:00:00.000' AS DateTime), N'Active', N'None')
--
INSERT [dbo].[CUSTOMER_OFFER] ([CustomerID], [OfferID]) VALUES(235892, 450)
INSERT [dbo].[CUSTOMER_OFFER] ([CustomerID], [OfferID]) VALUES(258279, 450)
INSERT [dbo].[CUSTOMER_OFFER] ([CustomerID], [OfferID]) VALUES(321799, 469)
--
INSERT [dbo].[ESTIMATED_WORTH] ([Plan_Code], [Date_effective], [Value_Amount]) VALUES(N'AVG', CAST(N'2016-06-06T00:00:00.000' AS DateTime), CAST(10 AS Numeric(18, 0)))
INSERT [dbo].[ESTIMATED_WORTH] ([Plan_Code], [Date_effective], [Value_Amount]) VALUES(N'HIGH', CAST(N'2011-01-01T00:00:00.000' AS DateTime), CAST(1545000 AS Numeric(18, 0)))
INSERT [dbo].[ESTIMATED_WORTH] ([Plan_Code], [Date_effective], [Value_Amount]) VALUES(N'LOW', CAST(N'2012-05-04T00:00:00.000' AS DateTime), CAST(450000 AS Numeric(18, 0)))
INSERT [dbo].[ESTIMATED_WORTH] ([Plan_Code], [Date_effective], [Value_Amount]) VALUES(N'LOW', CAST(N'2013-06-19T00:00:00.000' AS DateTime), CAST(550000 AS Numeric(18, 0)))
--
INSERT [dbo].[PERSONALISED_COSTING] ([Member], [Segment], [Plan_Code], [Date_effective], [Monthly_Cost]) VALUES(258279, N'LOW', N'HIGH', CAST(N'2014-01-01T00:00:00.000' AS DateTime), CAST(150 AS Numeric(18, 0)))
INSERT [dbo].[PERSONALISED_COSTING] ([Member], [Segment], [Plan_Code], [Date_effective], [Monthly_Cost]) VALUES(683492, N'HIGH', N'AVG', CAST(N'2013-01-01T00:00:00.000' AS DateTime), CAST(450 AS Numeric(18, 0)))
INSERT [dbo].[PERSONALISED_COSTING] ([Member], [Segment], [Plan_Code], [Date_effective], [Monthly_Cost]) VALUES(885325, N'MED', N'AVG', CAST(N'2013-01-01T00:00:00.000' AS DateTime), CAST(475 AS Numeric(18, 0)))