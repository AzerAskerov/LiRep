CREATE TABLE [dbo].[User](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Username] [varchar](32) NOT NULL,
	[Password] [varchar](128) NOT NULL,
	[Name] [nvarchar](128) NOT NULL,
	[Surname] [nvarchar](128) NOT NULL,
	[Role] [int] NOT NULL,
	[Supervisors] [varchar](128) NULL,
	[ProductGroup] [varchar](255) NULL

 CONSTRAINT [PK_dbo.User] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

INSERT INTO [dbo].[User] ([Username], [Password], [Name], [Surname], [Role], [Supervisors], [ProductGroup], [Email]) VALUES ('u1', 'a48d0fdeed454c588dffb78f21863832aae2ebda1d309c959b663e33fae3ffda', 'Ivan', 'Susanin', 49, '4,5')
GO
INSERT INTO [dbo].[User] ([Username], [Password], [Name], [Surname], [Role], [Supervisors], [ProductGroup], [Email]) VALUES ('u2', 'a48d0fdeed454c588dffb78f21863832aae2ebda1d309c959b663e33fae3ffda', 'Timur', 'Kasayev', 113, '4,5', '1')
GO
INSERT INTO [dbo].[User] ([Username], [Password], [Name], [Surname], [Role], [Supervisors], [ProductGroup], [Email]) VALUES ('u3', 'a48d0fdeed454c588dffb78f21863832aae2ebda1d309c959b663e33fae3ffda', 'Aslan', 'Mamedov', 241, '4,5')
GO
INSERT INTO [dbo].[User] ([Username], [Password], [Name], [Surname], [Role], [Supervisors], [ProductGroup], [Email]) VALUES ('u4', 'a48d0fdeed454c588dffb78f21863832aae2ebda1d309c959b663e33fae3ffda', 'Rafig', 'Babayev', 16384, null)
GO
INSERT INTO [dbo].[User] ([Username], [Password], [Name], [Surname], [Role], [Supervisors], [ProductGroup], [Email]) VALUES ('u5', 'a48d0fdeed454c588dffb78f21863832aae2ebda1d309c959b663e33fae3ffda', 'Ramiz', 'Samedov', 16384, null)
GO
INSERT INTO [dbo].[User] ([Username], [Password], [Name], [Surname], [Role], [Supervisors], [ProductGroup], [Email]) VALUES ('u6', 'a48d0fdeed454c588dffb78f21863832aae2ebda1d309c959b663e33fae3ffda', 'Muhammed', 'Abbasov', 3072, null)
GO
INSERT INTO [dbo].[User] ([Username], [Password], [Name], [Surname], [Role], [Supervisors], [ProductGroup], [Email]) VALUES ('u7', 'a48d0fdeed454c588dffb78f21863832aae2ebda1d309c959b663e33fae3ffda', 'Zaur', 'Askerov', 5120, null)
GO
INSERT INTO [dbo].[User] ([Username], [Password], [Name], [Surname], [Role], [Supervisors], [ProductGroup], [Email]) VALUES ('u8', 'a48d0fdeed454c588dffb78f21863832aae2ebda1d309c959b663e33fae3ffda', 'Rustam', 'Kasumov', 9216, null)
GO
INSERT INTO [dbo].[User] ([Username], [Password], [Name], [Surname], [Role], [Supervisors], [ProductGroup], [Email]) VALUES ('u9', 'a48d0fdeed454c588dffb78f21863832aae2ebda1d309c959b663e33fae3ffda', 'Ruslan', 'Abdulaev', 33304, null)
GO
INSERT INTO [dbo].[User] ([Username], [Password], [Name], [Surname], [Role], [Supervisors], [ProductGroup], [Email]) VALUES ('u0', 'a48d0fdeed454c588dffb78f21863832aae2ebda1d309c959b663e33fae3ffda', 'Fatih', 'Talibov', 131071, '10')
GO


CREATE TABLE [dbo].[VehicleBrand](
	[Id] [int] NOT NULL,
	[Name] [nvarchar](120) NOT NULL,
 CONSTRAINT [PK_VehicleBrand] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[Invoice](
	[Id] [varchar](32) NOT NULL,
	[PolicyNumber] [varchar](32) NOT NULL,
	[Product] [int] NOT NULL,
	[Brand] [int] NULL,
	[PolicyHolderType] [int] NOT NULL,
	[PolicyHolderCode] [varchar](32) NOT NULL,
	[PolicyHolderName] [nvarchar](512) NOT NULL,
	[BeneficiaryType] [int] NOT NULL,
	[BeneficiaryCode] [varchar](32) NOT NULL,
	[BeneficiaryName] [nvarchar](512) NOT NULL,
	[Premium] [decimal](8, 2) NOT NULL,
	[PayablePremium] [decimal](8, 2) NOT NULL,
	[CreateDate] [datetime] NOT NULL,
	[CreatorId] [int] NOT NULL,
	[ProcessStatus] [int] NOT NULL,
 CONSTRAINT [PK_dbo.Invoice] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[Invoice]  WITH CHECK ADD CONSTRAINT [FK_dbo.Invoice_dbo.User_CreatorId] FOREIGN KEY([CreatorId])
REFERENCES [dbo].[User] ([Id])
GO
ALTER TABLE [dbo].[Invoice] CHECK CONSTRAINT [FK_dbo.Invoice_dbo.User_CreatorId]
GO

ALTER TABLE [dbo].[Invoice]  WITH CHECK ADD  CONSTRAINT [FK_Invoice_VehicleBrand] FOREIGN KEY([Brand])
REFERENCES [dbo].[VehicleBrand] ([Id])
GO

ALTER TABLE [dbo].[Invoice] CHECK CONSTRAINT [FK_Invoice_VehicleBrand]
GO

CREATE UNIQUE NONCLUSTERED INDEX [UK_User_Username] ON [dbo].[User]
(
	[Username] ASC
)
GO

CREATE TABLE [dbo].[Payment](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[InvoiceId] [varchar](32) NOT NULL,
	[Amount] [decimal](8, 2) NOT NULL,
	[PayDate] [datetime] NOT NULL
 CONSTRAINT [PK_dbo.Payment] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

CREATE NONCLUSTERED INDEX [IX_Payment_InvoiceId] ON [dbo].[Payment]
(
	[InvoiceId] ASC
)
GO
ALTER TABLE [dbo].[Payment]  WITH CHECK ADD CONSTRAINT [FK_dbo.Payment_dbo.Invoice_InvoiceId] FOREIGN KEY([InvoiceId])
REFERENCES [dbo].[Invoice] ([Id])
GO
ALTER TABLE [dbo].[Payment] CHECK CONSTRAINT [FK_dbo.Payment_dbo.Invoice_InvoiceId]
GO

CREATE TABLE [dbo].[Commission](
	[InvoiceId] [varchar](32) NOT NULL,
	[PayoutType] [int] NOT NULL,
	[ActType] [int] NOT NULL,
	[Amount] [decimal](8, 2) NOT NULL
 CONSTRAINT [PK_dbo.Commission] PRIMARY KEY CLUSTERED 
(
	[InvoiceId] ASC, [PayoutType] ASC, [ActType] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[Commission]  WITH CHECK ADD CONSTRAINT [FK_dbo.Commission_dbo.Invoice_InvoiceId] FOREIGN KEY([InvoiceId])
REFERENCES [dbo].[Invoice] ([Id])
GO
ALTER TABLE [dbo].[Commission] CHECK CONSTRAINT [FK_dbo.Commission_dbo.Invoice_InvoiceId]
GO

CREATE TABLE [dbo].[Act](
	[Id] [varchar](32) NOT NULL,
	[Status] [int] NOT NULL,
	[Type] [int] NOT NULL,
	[CreatorId] [int] NOT NULL,
	[ReceiverId] [int] NOT NULL,
	[PayerId] [int] NULL,
	[CreateDate] [datetime] NOT NULL,
	[CancelDate] [datetime] NULL,	
	[Insurer] [int] NULL,
	[Broker] [int] NULL,
	[BrokerId]	int	NULL,
	[GroupId][int] NULL,
 CONSTRAINT [PK_dbo.Act] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
CREATE TABLE [dbo].[ProductDetail](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[GroupId] [int] NULL,
	[ProductIds] [nvarchar](255) NULL,
 CONSTRAINT [PK_ProductDetail] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[Act]  WITH CHECK ADD CONSTRAINT [FK_dbo.Act_dbo.User_CreatorId] FOREIGN KEY([CreatorId])
REFERENCES [dbo].[User] ([Id])
GO
ALTER TABLE [dbo].[Act] CHECK CONSTRAINT [FK_dbo.Act_dbo.User_CreatorId]
GO

ALTER TABLE [dbo].[Act]  WITH CHECK ADD CONSTRAINT [FK_dbo.Act_dbo.User_PayerId] FOREIGN KEY([PayerId])
REFERENCES [dbo].[User] ([Id])
GO
ALTER TABLE [dbo].[Act] CHECK CONSTRAINT [FK_dbo.Act_dbo.User_PayerId]
GO

CREATE TABLE [dbo].[ActCommission](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ActId] [varchar](32) NOT NULL,
	[InvoiceId] [varchar](32) NOT NULL,
	[PayoutType] [int] NOT NULL,
	[Amount] [decimal](8,2) NOT NULL,
 CONSTRAINT [PK_dbo.ActCommission] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[ActCommission]  WITH CHECK ADD CONSTRAINT [FK_dbo.ActCommission_dbo.Act_ActId] FOREIGN KEY([ActId])
REFERENCES [dbo].[Act] ([Id])
GO
ALTER TABLE [dbo].[ActCommission] CHECK CONSTRAINT [FK_dbo.ActCommission_dbo.Act_ActId]
GO

ALTER TABLE [dbo].[ActCommission]  WITH CHECK ADD CONSTRAINT [FK_dbo.ActCommission_dbo.Invoice_InvoiceId] FOREIGN KEY([InvoiceId])
REFERENCES [dbo].[Invoice] ([Id])
GO
ALTER TABLE [dbo].[ActCommission] CHECK CONSTRAINT [FK_dbo.ActCommission_dbo.Invoice_InvoiceId]
GO


CREATE TABLE [dbo].[Approval](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ActId] [varchar](32) NOT NULL,
	[ApproverId] [int] NULL,
	[ApproveDate] [datetime] NULL,
	[RejectDate] [datetime] NULL,
	[RejectNote] [nvarchar](200) NULL,
 CONSTRAINT [PK_dbo.Approval] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[Approval]  WITH CHECK ADD CONSTRAINT [FK_dbo.Approval_dbo.Act_ActId] FOREIGN KEY([ActId])
REFERENCES [dbo].[Act] ([Id])
GO
ALTER TABLE [dbo].[Approval] CHECK CONSTRAINT [FK_dbo.Approval_dbo.Act_ActId]
GO

ALTER TABLE [dbo].[Approval]  WITH CHECK ADD CONSTRAINT [FK_dbo.Approval_dbo.User_ApproverId] FOREIGN KEY([ApproverId])
REFERENCES [dbo].[User] ([Id])
GO
ALTER TABLE [dbo].[Approval] CHECK CONSTRAINT [FK_dbo.Approval_dbo.User_ApproverId]
GO

CREATE TABLE [dbo].[Payout](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ActId] [varchar](32) NOT NULL,
	[Type] [int] NOT NULL,
	[Amount] [decimal](8,2) NOT NULL,
	[PayerId] [int] NULL,
	[PayDate] [datetime] NULL
 CONSTRAINT [PK_dbo.Payout] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[Payout]  WITH CHECK ADD CONSTRAINT [FK_dbo.Payout_dbo.Act_ActId] FOREIGN KEY([ActId])
REFERENCES [dbo].[Act] ([Id])
GO
ALTER TABLE [dbo].[Payout] CHECK CONSTRAINT [FK_dbo.Payout_dbo.Act_ActId]
GO

ALTER TABLE [dbo].[Payout]  WITH CHECK ADD CONSTRAINT [FK_dbo.Payout_dbo.User_PayerId] FOREIGN KEY([PayerId])
REFERENCES [dbo].[User] ([Id])
GO
ALTER TABLE [dbo].[Payout] CHECK CONSTRAINT [FK_dbo.Payout_dbo.User_PayerId]
GO

CREATE TABLE [dbo].[CommissionConfig](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ActType] [int] NOT NULL,
	[PayoutType] [int] NOT NULL,
	[Product] [int] NULL,
	[Brand] [int] NULL,
	[PolicyHolderType] [int] NULL,
	[Username] [varchar](32) NULL,
	[BeneficiaryCode] [nvarchar](32) NULL,
	[AmountFixed] [decimal](8, 2) NOT NULL,
	[AmountPercent] [decimal](8, 2) NOT NULL,
	[AmountMin] [decimal](8, 2) NULL,
	[AmountMax] [decimal](8, 2) NULL,
	[ValidFrom] [datetime] NOT NULL,
	[ValidTo] [datetime] NOT NULL,
	[PremiumFrom] [decimal](8, 0) NOT NULL,
	[PremiumTo] [decimal](8, 0) NOT NULL,
 CONSTRAINT [PK_dbo.CommissionConfig] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[CommissionConfig] ADD  CONSTRAINT [DF_CommissionConfig_PremiumFrom]  DEFAULT ((0)) FOR [PremiumFrom]
GO

ALTER TABLE [dbo].[CommissionConfig] ADD  CONSTRAINT [DF_CommissionConfig_PremiumTo]  DEFAULT ((999999)) FOR [PremiumTo]
GO

ALTER TABLE [dbo].[CommissionConfig]  WITH CHECK ADD  CONSTRAINT [FK_CommissionConfig_VehicleBrand] FOREIGN KEY([Brand])
REFERENCES [dbo].[VehicleBrand] ([Id])
GO

ALTER TABLE [dbo].[CommissionConfig] CHECK CONSTRAINT [FK_CommissionConfig_VehicleBrand]
GO


CREATE TABLE [dbo].[CommonWebApiLog](
	[LogOid] [int] IDENTITY(1,1) NOT NULL,
	[Timestamp] [datetime] NOT NULL,
	[Path] [varchar](200) NULL,
	[SourceIp] [varchar](40) NULL,
	[Request] [nvarchar](max) NULL,
	[Response] [nvarchar](max) NULL,
	[ResponseCode] [smallint] NULL,
	[UserId] [int] NULL,
	[ObjectIdentifier] [nvarchar](1000) NULL,
 CONSTRAINT [CommonWebApiLog_PK] PRIMARY KEY NONCLUSTERED 
(
	[LogOid] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO


CREATE TABLE [dbo].[RecalculatedInvoiceLog](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[InvoiceId] [varchar](32) NOT NULL,
	[Status] [varchar](9) NULL,
	[ErrorDescription] [nvarchar](max) NULL,
	[Timestamp] [datetime] NOT NULL,
 CONSTRAINT [PK_RecalculatedInvoiceLog] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[RecalculatedInvoiceLog] ADD  CONSTRAINT [DF_RecalculatedInvoiceLog_Timestamp]  DEFAULT (getdate()) FOR [Timestamp]
GO




CREATE view [dbo].[InvoiceView]
as
select 
	i.Id,
	i.PolicyNumber,
	i.PolicyHolderCode + ', ' + i.PolicyHolderName as PolicyHolder,
	i.Product,
	v.[Name] as Brand,
	i.BeneficiaryCode + ', ' + i.BeneficiaryName as Beneficiary,
	i.BeneficiaryCode,
	i.Premium,
	i.PayablePremium,
	i.CreateDate,
	max(p.PayDate) as PayDate,
	i.CreatorId,
	u.[Name] + ' ' + u.Surname + ' (' + UPPER(u.Username) + ')' as Creator,
	u.Username as CreatorCode,
	i.ProcessStatus,
	sum(p.Amount) as PaidPremium,
	i.PayablePremium - sum(p.Amount) as UnpaidPremium,
	i.Premium - i.PayablePremium as WithheldCommission
from dbo.Invoice as i
inner join dbo.Payment as p on p.InvoiceId = i.Id
inner join dbo.[User] as u on u.Id = i.CreatorId
left join dbo.[VehicleBrand] as v on v.id = i.brand
group by i.Id, i.ProcessStatus, i.CreatorId, u.[Name], u.Surname, u.Username, i.CreateDate, i.PolicyNumber, 
		 i.Product, v.[Name], i.PolicyHolderCode, i.PolicyHolderName, i.BeneficiaryCode, i.BeneficiaryName, i.Premium, i.PayablePremium 

GO

CREATE VIEW [dbo].[CommissionView]
AS
SELECT        c.InvoiceId, c.[PayoutType], c.[ActType], c.Amount AS TotalAmount, round(c.Amount * 100 / i.PayablePremium, 0) AS TotalPercent, paid.Amount AS PaidAmount, round(paid.Amount * 100 / i.PayablePremium, 0) AS PaidPercent, 
                         CASE WHEN c.Amount > paid.Amount THEN c.Amount - paid.Amount ELSE 0 END AS UnpaidAmount, round(CASE WHEN c.Amount > paid.Amount THEN c.Amount - paid.Amount ELSE 0 END * 100 / i.PayablePremium, 0) 
                         AS UnpaidPercent, CASE WHEN i.PayablePremium = sum(p.Amount) THEN CASE WHEN c.Amount > paid.Amount THEN c.Amount - paid.Amount ELSE 0 END ELSE CASE WHEN round(sum(p.Amount) 
                         / i.PayablePremium * c.Amount - paid.Amount, 2) < 0 THEN 0 ELSE round(sum(p.Amount) / i.PayablePremium * c.Amount - paid.Amount, 2) END END AS CustomAmount, round(c.Amount * 100 / i.PayablePremium, 0) 
                         AS CustomPercent, cast(IIF(mc.InvoiceId is null,0,1) as bit) IsManual
FROM            dbo.Invoice AS i INNER JOIN
                         dbo.Commission AS c ON c.InvoiceId = i.Id left join 
dbo.ManualCommission mc on mc.InvoiceId = c.InvoiceId and mc.ActType=c.ActType and mc.PayoutType=c.PayoutType	INNER JOIN
                         dbo.Payment AS p ON p.InvoiceId = i.Id OUTER apply
                             (SELECT        isnull(sum(ac.Amount), 0) AS Amount
                               FROM            dbo.ActCommission AS ac INNER JOIN
                                                         dbo.Act AS a ON a.Id = ac.ActId AND a.[Status] IN (4, 5) AND a.[Type] = c.[ActType]
                               WHERE        InvoiceId = i.Id AND ac.[PayoutType] = c.[PayoutType]) AS paid
GROUP BY c.InvoiceId, c.[PayoutType], c.ActType, c.Amount, i.PayablePremium, paid.Amount, mc.InvoiceId
GO


create view dbo.ActInvoiceView
as
select 
	i.Id,
	a.Id as ActId,
	i.PolicyNumber,
	i.PolicyHolderCode + ', ' + i.PolicyHolderName as PolicyHolder,
	i.Product,
	v.Name as [Brand],
	i.BeneficiaryCode + ', ' + i.BeneficiaryName as Beneficiary,
	i.Premium,
	i.PayablePremium,
	i.CreateDate,
	i.CreatorId,
	u.[Name] + ' ' + u.Surname as Creator,
	i.ProcessStatus,
	sum(p.Amount) as PaidPremium,
	i.PayablePremium - sum(p.Amount) as UnpaidPremium,
	i.Premium - i.PayablePremium as WithheldCommission
from dbo.Invoice as i
cross apply (select ActId from dbo.ActCommission as x where x.InvoiceId = i.Id group by ActId) as ac
inner join dbo.Act as a on a.Id = ac.ActId
inner join dbo.Payment as p on p.InvoiceId = i.Id
inner join dbo.[User] as u on u.Id = i.CreatorId
left join dbo.[VehicleBrand] as v on v.id = i.brand
group by i.Id, a.Id, p.Id, i.ProcessStatus, i.CreatorId, u.[Name], u.Surname, i.CreateDate, 
	     i.PolicyNumber, i.Product, v.[Name], i.PolicyHolderCode, i.PolicyHolderName, i.BeneficiaryCode, i.BeneficiaryName, i.Premium, i.PayablePremium 

Go

create view dbo.ActCommissionView
as
select 
	ac.InvoiceId,
	ac.ActId,
	ac.[PayoutType],
	a.[Type] as ActType,
	c.Amount as TotalAmount,
	c.Amount * 100 / i.PayablePremium as TotalPercent,
	paid.Amount as PaidAmount,
	paid.Amount * 100 / i.PayablePremium as PaidPercent,
	case when c.Amount > paid.Amount then c.Amount - paid.Amount else 0 end as UnpaidAmount,
	case when c.Amount > paid.Amount then c.Amount - paid.Amount else 0 end * 100 / i.PayablePremium as UnpaidPercent,
	ac.Amount as CustomAmount,
	ac.Amount * 100 / i.PayablePremium as  CustomPercent 
from dbo.ActCommission as ac
inner join dbo.Act as a on a.Id = ac.ActId
inner join dbo.Commission as c on c.InvoiceId = ac.InvoiceId and c.[PayoutType] = ac.[PayoutType] and c.[ActType] = a.[Type]
inner join dbo.Invoice as i on i.Id = ac.InvoiceId
outer apply (select isnull(sum(ac1.Amount), 0) as Amount 
             from dbo.ActCommission as ac1
			 inner join dbo.Act as a1 on a1.Id = ac1.ActId and a1.[Status] in (4,5) and a1.[Type] = a.[Type]
			 where InvoiceId = i.Id and ac1.[PayoutType] = c.[PayoutType]) as paid

go

create view dbo.ActView
as
select 
	a.Id,
	a.[Status],
	a.CreatorId,
	a.CreateDate,
	r.Username as ReceiverCode,
	sum(c.Amount) as Amount
from dbo.Act as a
inner join dbo.ActCommission as c on c.ActId = a.Id
inner join dbo.[User] as r on r.Id = a.ReceiverId
group by a.Id, a.[Status], a.CreatorId, a.CreateDate, r.Username

go

create view dbo.ActApprovalView
as
select 
	a.Id,
	a.[Status],
	a.CreatorId,
	a.CreateDate,
	ap.ApproverId,
	ap.ApproveDate,
	ap.RejectDate,
	u.[Name] + ' ' + u.Surname as Creator,
	sum(c.Amount) as Amount,
	a.GroupId
from dbo.Act as a
inner join dbo.ActCommission as c on c.ActId = a.Id
inner join dbo.Approval as ap on ap.ActId = a.Id
inner join dbo.[User] as u on u.Id = a.CreatorId 
left join dbo.ProductDetail as pd on pd.GroupId=a.GroupId
group by a.Id, a.[Status], a.CreatorId, a.CreateDate, a.GroupId, ap.ApproverId, ap.ApproveDate, ap.RejectDate, u.[Name], u.Surname

GO


create view dbo.PayoutInvoiceView
as
select 
	p.Id,
	i.Id as InvoiceId,
	p.Id as PayoutId,
	i.PolicyNumber,
	i.PolicyHolderCode + ', ' + i.PolicyHolderName as PolicyHolder,
	c.Amount
from dbo.Payout as p
inner join dbo.Act as a on a.Id = p.ActId
inner join dbo.ActCommission as c on c.ActId = p.ActId and c.[PayoutType] = p.[Type]
inner join dbo.Invoice as i on i.Id = c.InvoiceId

GO

