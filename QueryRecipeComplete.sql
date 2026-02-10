USE [AgroConnect]
GO
/****** Object:  Table [dbo].[Advisors]    Script Date: 9/2/2026 22:40:32 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Advisors](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[FullName] [nvarchar](200) NOT NULL,
	[LicenseNumber] [nvarchar](50) NOT NULL,
	[Contact] [nvarchar](200) NULL,
	[CreatedAt] [datetime2](0) NOT NULL,
	[UpdatedAt] [datetime2](0) NULL,
	[DeletedAt] [datetime2](0) NULL,
	[CreatedByUserId] [bigint] NULL,
	[UpdatedByUserId] [bigint] NULL,
	[DeletedByUserId] [bigint] NULL,
 CONSTRAINT [PK_Advisors] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Products]    Script Date: 9/2/2026 22:40:32 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Products](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[SenasaRegistry] [nvarchar](50) NULL,
	[ProductName] [nvarchar](200) NOT NULL,
	[ToxicologicalClass] [nvarchar](100) NULL,
	[CreatedAt] [datetime2](0) NOT NULL,
	[UpdatedAt] [datetime2](0) NULL,
	[DeletedAt] [datetime2](0) NULL,
	[CreatedByUserId] [bigint] NULL,
	[UpdatedByUserId] [bigint] NULL,
	[DeletedByUserId] [bigint] NULL,
 CONSTRAINT [PK_Products] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecipeLots]    Script Date: 9/2/2026 22:40:32 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecipeLots](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[RecipeId] [bigint] NOT NULL,
	[LotName] [nvarchar](200) NOT NULL,
	[Locality] [nvarchar](150) NULL,
	[Department] [nvarchar](150) NULL,
	[SurfaceHa] [decimal](10, 2) NULL,
	[CreatedAt] [datetime2](0) NOT NULL,
	[UpdatedAt] [datetime2](0) NULL,
	[DeletedAt] [datetime2](0) NULL,
	[CreatedByUserId] [bigint] NULL,
	[UpdatedByUserId] [bigint] NULL,
	[DeletedByUserId] [bigint] NULL,
 CONSTRAINT [PK_RecipeLots] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecipeLotVertices]    Script Date: 9/2/2026 22:40:32 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecipeLotVertices](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[LotId] [bigint] NOT NULL,
	[Order] [int] NOT NULL,
	[Latitude] [decimal](10, 7) NOT NULL,
	[Longitude] [decimal](10, 7) NOT NULL,
	[CreatedAt] [datetime2](0) NOT NULL,
	[UpdatedAt] [datetime2](0) NULL,
	[DeletedAt] [datetime2](0) NULL,
 CONSTRAINT [PK_RecipeLotVertices] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecipeProducts]    Script Date: 9/2/2026 22:40:32 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecipeProducts](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[RecipeId] [bigint] NOT NULL,
	[ProductType] [nvarchar](50) NULL,
	[ProductName] [nvarchar](200) NOT NULL,
	[SenasaRegistry] [nvarchar](50) NULL,
	[ToxicologicalClass] [nvarchar](100) NULL,
	[DoseValue] [decimal](18, 6) NULL,
	[DoseUnit] [nvarchar](30) NULL,
	[DosePerUnit] [nvarchar](30) NULL,
	[TotalValue] [decimal](18, 6) NULL,
	[TotalUnit] [nvarchar](30) NULL,
	[CreatedAt] [datetime2](0) NOT NULL,
	[UpdatedAt] [datetime2](0) NULL,
	[DeletedAt] [datetime2](0) NULL,
	[CreatedByUserId] [bigint] NULL,
	[UpdatedByUserId] [bigint] NULL,
	[DeletedByUserId] [bigint] NULL,
	[ProductId] [bigint] NOT NULL,
 CONSTRAINT [PK_RecipeProducts] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Recipes]    Script Date: 9/2/2026 22:40:32 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Recipes](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[RfdNumber] [bigint] NOT NULL,
	[Status] [nvarchar](30) NOT NULL,
	[IssueDate] [date] NOT NULL,
	[PossibleStartDate] [date] NULL,
	[RecommendedDate] [date] NULL,
	[ExpirationDate] [date] NULL,
	[RequesterId] [bigint] NOT NULL,
	[AdvisorId] [bigint] NOT NULL,
	[ApplicationType] [nvarchar](100) NULL,
	[Crop] [nvarchar](150) NULL,
	[Diagnosis] [nvarchar](150) NULL,
	[Treatment] [nvarchar](150) NULL,
	[MachineToUse] [nvarchar](100) NULL,
	[MachinePlate] [nvarchar](50) NULL,
	[MachineLegalName] [nvarchar](200) NULL,
	[MachineType] [nvarchar](100) NULL,
	[UnitSurfaceHa] [decimal](10, 2) NULL,
	[TempMin] [decimal](10, 2) NULL,
	[TempMax] [decimal](10, 2) NULL,
	[HumidityMin] [decimal](10, 2) NULL,
	[HumidityMax] [decimal](10, 2) NULL,
	[WindMinKmh] [decimal](10, 2) NULL,
	[WindMaxKmh] [decimal](10, 2) NULL,
	[WindDirection] [nvarchar](50) NULL,
	[Notes] [nvarchar](max) NULL,
	[CreatedAt] [datetime2](0) NOT NULL,
	[UpdatedAt] [datetime2](0) NULL,
	[DeletedAt] [datetime2](0) NULL,
	[CreatedByUserId] [bigint] NULL,
	[UpdatedByUserId] [bigint] NULL,
	[DeletedByUserId] [bigint] NULL,
 CONSTRAINT [PK_Recipes] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecipeSensitivePoints]    Script Date: 9/2/2026 22:40:32 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecipeSensitivePoints](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[RecipeId] [bigint] NOT NULL,
	[Name] [nvarchar](200) NOT NULL,
	[Type] [nvarchar](100) NULL,
	[Locality] [nvarchar](150) NULL,
	[Department] [nvarchar](150) NULL,
	[Latitude] [decimal](10, 7) NULL,
	[Longitude] [decimal](10, 7) NULL,
	[CreatedAt] [datetime2](0) NOT NULL,
	[UpdatedAt] [datetime2](0) NULL,
	[DeletedAt] [datetime2](0) NULL,
	[CreatedByUserId] [bigint] NULL,
	[UpdatedByUserId] [bigint] NULL,
	[DeletedByUserId] [bigint] NULL,
 CONSTRAINT [PK_RecipeSensitivePoints] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecipeStatusHistory]    Script Date: 9/2/2026 22:40:32 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecipeStatusHistory](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[RecipeId] [bigint] NOT NULL,
	[OldStatus] [nvarchar](30) NULL,
	[NewStatus] [nvarchar](30) NOT NULL,
	[ChangedAt] [datetime2](0) NOT NULL,
	[ChangedByUserId] [bigint] NULL,
	[Source] [nvarchar](50) NULL,
	[Notes] [nvarchar](300) NULL,
 CONSTRAINT [PK_RecipeStatusHistory] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Requesters]    Script Date: 9/2/2026 22:40:32 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Requesters](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[LegalName] [nvarchar](200) NOT NULL,
	[TaxId] [nvarchar](20) NOT NULL,
	[Address] [nvarchar](300) NULL,
	[Contact] [nvarchar](200) NULL,
	[CreatedAt] [datetime2](0) NOT NULL,
	[UpdatedAt] [datetime2](0) NULL,
	[DeletedAt] [datetime2](0) NULL,
	[CreatedByUserId] [bigint] NULL,
	[UpdatedByUserId] [bigint] NULL,
	[DeletedByUserId] [bigint] NULL,
 CONSTRAINT [PK_Requesters] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Roles]    Script Date: 9/2/2026 22:40:32 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Roles](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[AccessLevel] [smallint] NOT NULL,
	[Description] [nvarchar](300) NULL,
	[CreatedAt] [datetime2](0) NOT NULL,
	[UpdatedAt] [datetime2](0) NULL,
	[DeletedAt] [datetime2](0) NULL,
	[CreatedByUserId] [bigint] NULL,
	[UpdatedByUserId] [bigint] NULL,
	[DeletedByUserId] [bigint] NULL,
 CONSTRAINT [PK_Roles] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[UserRoles]    Script Date: 9/2/2026 22:40:32 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UserRoles](
	[UserId] [bigint] NOT NULL,
	[RoleId] [bigint] NOT NULL,
	[CreatedAt] [datetime2](0) NOT NULL,
 CONSTRAINT [PK_UserRoles] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC,
	[RoleId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Users]    Script Date: 9/2/2026 22:40:32 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Users](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[UserName] [nvarchar](100) NOT NULL,
	[EmailNormalized] [nvarchar](256) NOT NULL,
	[PasswordHash] [nvarchar](500) NOT NULL,
	[IsBlocked] [bit] NOT NULL,
	[LastLoginAt] [datetime2](0) NULL,
	[CreatedAt] [datetime2](0) NOT NULL,
	[UpdatedAt] [datetime2](0) NULL,
	[DeletedAt] [datetime2](0) NULL,
	[CreatedByUserId] [bigint] NULL,
	[UpdatedByUserId] [bigint] NULL,
	[DeletedByUserId] [bigint] NULL,
	[PhoneNumber] [nvarchar](30) NULL,
	[TaxId] [nvarchar](20) NULL,
 CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Advisors] ADD  CONSTRAINT [DF_Advisors_CreatedAt]  DEFAULT (sysdatetime()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[Products] ADD  CONSTRAINT [DF_Products_CreatedAt]  DEFAULT (sysdatetime()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[RecipeLots] ADD  CONSTRAINT [DF_RecipeLots_CreatedAt]  DEFAULT (sysdatetime()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[RecipeLotVertices] ADD  CONSTRAINT [DF_RecipeLotVertices_CreatedAt]  DEFAULT (sysdatetime()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[RecipeProducts] ADD  CONSTRAINT [DF_RecipeProducts_CreatedAt]  DEFAULT (sysdatetime()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[Recipes] ADD  CONSTRAINT [DF_Recipes_CreatedAt]  DEFAULT (sysdatetime()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[RecipeSensitivePoints] ADD  CONSTRAINT [DF_RecipeSensitivePoints_CreatedAt]  DEFAULT (sysdatetime()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[RecipeStatusHistory] ADD  CONSTRAINT [DF_RecipeStatusHistory_ChangedAt]  DEFAULT (sysutcdatetime()) FOR [ChangedAt]
GO
ALTER TABLE [dbo].[Requesters] ADD  CONSTRAINT [DF_Requesters_CreatedAt]  DEFAULT (sysdatetime()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[Roles] ADD  CONSTRAINT [DF_Roles_AccessLevel]  DEFAULT ((0)) FOR [AccessLevel]
GO
ALTER TABLE [dbo].[Roles] ADD  CONSTRAINT [DF_Roles_CreatedAt]  DEFAULT (sysdatetime()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[UserRoles] ADD  CONSTRAINT [DF_UserRoles_CreatedAt]  DEFAULT (sysdatetime()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[Users] ADD  CONSTRAINT [DF_Users_IsBlocked]  DEFAULT ((0)) FOR [IsBlocked]
GO
ALTER TABLE [dbo].[Users] ADD  CONSTRAINT [DF_Users_CreatedAt]  DEFAULT (sysdatetime()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[Advisors]  WITH CHECK ADD  CONSTRAINT [FK_Advisors_CreatedByUser] FOREIGN KEY([CreatedByUserId])
REFERENCES [dbo].[Users] ([Id])
GO
ALTER TABLE [dbo].[Advisors] CHECK CONSTRAINT [FK_Advisors_CreatedByUser]
GO
ALTER TABLE [dbo].[Advisors]  WITH CHECK ADD  CONSTRAINT [FK_Advisors_DeletedByUser] FOREIGN KEY([DeletedByUserId])
REFERENCES [dbo].[Users] ([Id])
GO
ALTER TABLE [dbo].[Advisors] CHECK CONSTRAINT [FK_Advisors_DeletedByUser]
GO
ALTER TABLE [dbo].[Advisors]  WITH CHECK ADD  CONSTRAINT [FK_Advisors_UpdatedByUser] FOREIGN KEY([UpdatedByUserId])
REFERENCES [dbo].[Users] ([Id])
GO
ALTER TABLE [dbo].[Advisors] CHECK CONSTRAINT [FK_Advisors_UpdatedByUser]
GO
ALTER TABLE [dbo].[RecipeLots]  WITH CHECK ADD  CONSTRAINT [FK_RecipeLots_CreatedByUser] FOREIGN KEY([CreatedByUserId])
REFERENCES [dbo].[Users] ([Id])
GO
ALTER TABLE [dbo].[RecipeLots] CHECK CONSTRAINT [FK_RecipeLots_CreatedByUser]
GO
ALTER TABLE [dbo].[RecipeLots]  WITH CHECK ADD  CONSTRAINT [FK_RecipeLots_DeletedByUser] FOREIGN KEY([DeletedByUserId])
REFERENCES [dbo].[Users] ([Id])
GO
ALTER TABLE [dbo].[RecipeLots] CHECK CONSTRAINT [FK_RecipeLots_DeletedByUser]
GO
ALTER TABLE [dbo].[RecipeLots]  WITH CHECK ADD  CONSTRAINT [FK_RecipeLots_Recipes] FOREIGN KEY([RecipeId])
REFERENCES [dbo].[Recipes] ([Id])
GO
ALTER TABLE [dbo].[RecipeLots] CHECK CONSTRAINT [FK_RecipeLots_Recipes]
GO
ALTER TABLE [dbo].[RecipeLots]  WITH CHECK ADD  CONSTRAINT [FK_RecipeLots_UpdatedByUser] FOREIGN KEY([UpdatedByUserId])
REFERENCES [dbo].[Users] ([Id])
GO
ALTER TABLE [dbo].[RecipeLots] CHECK CONSTRAINT [FK_RecipeLots_UpdatedByUser]
GO
ALTER TABLE [dbo].[RecipeLotVertices]  WITH CHECK ADD  CONSTRAINT [FK_RecipeLotVertices_RecipeLots] FOREIGN KEY([LotId])
REFERENCES [dbo].[RecipeLots] ([Id])
GO
ALTER TABLE [dbo].[RecipeLotVertices] CHECK CONSTRAINT [FK_RecipeLotVertices_RecipeLots]
GO
ALTER TABLE [dbo].[RecipeProducts]  WITH CHECK ADD  CONSTRAINT [FK_RecipeProducts_CreatedByUser] FOREIGN KEY([CreatedByUserId])
REFERENCES [dbo].[Users] ([Id])
GO
ALTER TABLE [dbo].[RecipeProducts] CHECK CONSTRAINT [FK_RecipeProducts_CreatedByUser]
GO
ALTER TABLE [dbo].[RecipeProducts]  WITH CHECK ADD  CONSTRAINT [FK_RecipeProducts_DeletedByUser] FOREIGN KEY([DeletedByUserId])
REFERENCES [dbo].[Users] ([Id])
GO
ALTER TABLE [dbo].[RecipeProducts] CHECK CONSTRAINT [FK_RecipeProducts_DeletedByUser]
GO
ALTER TABLE [dbo].[RecipeProducts]  WITH CHECK ADD  CONSTRAINT [FK_RecipeProducts_Products] FOREIGN KEY([ProductId])
REFERENCES [dbo].[Products] ([Id])
GO
ALTER TABLE [dbo].[RecipeProducts] CHECK CONSTRAINT [FK_RecipeProducts_Products]
GO
ALTER TABLE [dbo].[RecipeProducts]  WITH CHECK ADD  CONSTRAINT [FK_RecipeProducts_Recipes] FOREIGN KEY([RecipeId])
REFERENCES [dbo].[Recipes] ([Id])
GO
ALTER TABLE [dbo].[RecipeProducts] CHECK CONSTRAINT [FK_RecipeProducts_Recipes]
GO
ALTER TABLE [dbo].[RecipeProducts]  WITH CHECK ADD  CONSTRAINT [FK_RecipeProducts_UpdatedByUser] FOREIGN KEY([UpdatedByUserId])
REFERENCES [dbo].[Users] ([Id])
GO
ALTER TABLE [dbo].[RecipeProducts] CHECK CONSTRAINT [FK_RecipeProducts_UpdatedByUser]
GO
ALTER TABLE [dbo].[Recipes]  WITH CHECK ADD  CONSTRAINT [FK_Recipes_Advisors] FOREIGN KEY([AdvisorId])
REFERENCES [dbo].[Advisors] ([Id])
GO
ALTER TABLE [dbo].[Recipes] CHECK CONSTRAINT [FK_Recipes_Advisors]
GO
ALTER TABLE [dbo].[Recipes]  WITH CHECK ADD  CONSTRAINT [FK_Recipes_CreatedByUser] FOREIGN KEY([CreatedByUserId])
REFERENCES [dbo].[Users] ([Id])
GO
ALTER TABLE [dbo].[Recipes] CHECK CONSTRAINT [FK_Recipes_CreatedByUser]
GO
ALTER TABLE [dbo].[Recipes]  WITH CHECK ADD  CONSTRAINT [FK_Recipes_DeletedByUser] FOREIGN KEY([DeletedByUserId])
REFERENCES [dbo].[Users] ([Id])
GO
ALTER TABLE [dbo].[Recipes] CHECK CONSTRAINT [FK_Recipes_DeletedByUser]
GO
ALTER TABLE [dbo].[Recipes]  WITH CHECK ADD  CONSTRAINT [FK_Recipes_Requesters] FOREIGN KEY([RequesterId])
REFERENCES [dbo].[Requesters] ([Id])
GO
ALTER TABLE [dbo].[Recipes] CHECK CONSTRAINT [FK_Recipes_Requesters]
GO
ALTER TABLE [dbo].[Recipes]  WITH CHECK ADD  CONSTRAINT [FK_Recipes_UpdatedByUser] FOREIGN KEY([UpdatedByUserId])
REFERENCES [dbo].[Users] ([Id])
GO
ALTER TABLE [dbo].[Recipes] CHECK CONSTRAINT [FK_Recipes_UpdatedByUser]
GO
ALTER TABLE [dbo].[RecipeSensitivePoints]  WITH CHECK ADD  CONSTRAINT [FK_RecipeSensitivePoints_CreatedByUser] FOREIGN KEY([CreatedByUserId])
REFERENCES [dbo].[Users] ([Id])
GO
ALTER TABLE [dbo].[RecipeSensitivePoints] CHECK CONSTRAINT [FK_RecipeSensitivePoints_CreatedByUser]
GO
ALTER TABLE [dbo].[RecipeSensitivePoints]  WITH CHECK ADD  CONSTRAINT [FK_RecipeSensitivePoints_DeletedByUser] FOREIGN KEY([DeletedByUserId])
REFERENCES [dbo].[Users] ([Id])
GO
ALTER TABLE [dbo].[RecipeSensitivePoints] CHECK CONSTRAINT [FK_RecipeSensitivePoints_DeletedByUser]
GO
ALTER TABLE [dbo].[RecipeSensitivePoints]  WITH CHECK ADD  CONSTRAINT [FK_RecipeSensitivePoints_Recipes] FOREIGN KEY([RecipeId])
REFERENCES [dbo].[Recipes] ([Id])
GO
ALTER TABLE [dbo].[RecipeSensitivePoints] CHECK CONSTRAINT [FK_RecipeSensitivePoints_Recipes]
GO
ALTER TABLE [dbo].[RecipeSensitivePoints]  WITH CHECK ADD  CONSTRAINT [FK_RecipeSensitivePoints_UpdatedByUser] FOREIGN KEY([UpdatedByUserId])
REFERENCES [dbo].[Users] ([Id])
GO
ALTER TABLE [dbo].[RecipeSensitivePoints] CHECK CONSTRAINT [FK_RecipeSensitivePoints_UpdatedByUser]
GO
ALTER TABLE [dbo].[RecipeStatusHistory]  WITH CHECK ADD  CONSTRAINT [FK_RecipeStatusHistory_Recipes] FOREIGN KEY([RecipeId])
REFERENCES [dbo].[Recipes] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[RecipeStatusHistory] CHECK CONSTRAINT [FK_RecipeStatusHistory_Recipes]
GO
ALTER TABLE [dbo].[Requesters]  WITH CHECK ADD  CONSTRAINT [FK_Requesters_CreatedByUser] FOREIGN KEY([CreatedByUserId])
REFERENCES [dbo].[Users] ([Id])
GO
ALTER TABLE [dbo].[Requesters] CHECK CONSTRAINT [FK_Requesters_CreatedByUser]
GO
ALTER TABLE [dbo].[Requesters]  WITH CHECK ADD  CONSTRAINT [FK_Requesters_DeletedByUser] FOREIGN KEY([DeletedByUserId])
REFERENCES [dbo].[Users] ([Id])
GO
ALTER TABLE [dbo].[Requesters] CHECK CONSTRAINT [FK_Requesters_DeletedByUser]
GO
ALTER TABLE [dbo].[Requesters]  WITH CHECK ADD  CONSTRAINT [FK_Requesters_UpdatedByUser] FOREIGN KEY([UpdatedByUserId])
REFERENCES [dbo].[Users] ([Id])
GO
ALTER TABLE [dbo].[Requesters] CHECK CONSTRAINT [FK_Requesters_UpdatedByUser]
GO
ALTER TABLE [dbo].[Roles]  WITH CHECK ADD  CONSTRAINT [FK_Roles_CreatedByUser] FOREIGN KEY([CreatedByUserId])
REFERENCES [dbo].[Users] ([Id])
GO
ALTER TABLE [dbo].[Roles] CHECK CONSTRAINT [FK_Roles_CreatedByUser]
GO
ALTER TABLE [dbo].[Roles]  WITH CHECK ADD  CONSTRAINT [FK_Roles_DeletedByUser] FOREIGN KEY([DeletedByUserId])
REFERENCES [dbo].[Users] ([Id])
GO
ALTER TABLE [dbo].[Roles] CHECK CONSTRAINT [FK_Roles_DeletedByUser]
GO
ALTER TABLE [dbo].[Roles]  WITH CHECK ADD  CONSTRAINT [FK_Roles_UpdatedByUser] FOREIGN KEY([UpdatedByUserId])
REFERENCES [dbo].[Users] ([Id])
GO
ALTER TABLE [dbo].[Roles] CHECK CONSTRAINT [FK_Roles_UpdatedByUser]
GO
ALTER TABLE [dbo].[UserRoles]  WITH CHECK ADD  CONSTRAINT [FK_UserRoles_Roles] FOREIGN KEY([RoleId])
REFERENCES [dbo].[Roles] ([Id])
GO
ALTER TABLE [dbo].[UserRoles] CHECK CONSTRAINT [FK_UserRoles_Roles]
GO
ALTER TABLE [dbo].[UserRoles]  WITH CHECK ADD  CONSTRAINT [FK_UserRoles_Users] FOREIGN KEY([UserId])
REFERENCES [dbo].[Users] ([Id])
GO
ALTER TABLE [dbo].[UserRoles] CHECK CONSTRAINT [FK_UserRoles_Users]
GO
ALTER TABLE [dbo].[Users]  WITH CHECK ADD  CONSTRAINT [FK_Users_CreatedByUser] FOREIGN KEY([CreatedByUserId])
REFERENCES [dbo].[Users] ([Id])
GO
ALTER TABLE [dbo].[Users] CHECK CONSTRAINT [FK_Users_CreatedByUser]
GO
ALTER TABLE [dbo].[Users]  WITH CHECK ADD  CONSTRAINT [FK_Users_DeletedByUser] FOREIGN KEY([DeletedByUserId])
REFERENCES [dbo].[Users] ([Id])
GO
ALTER TABLE [dbo].[Users] CHECK CONSTRAINT [FK_Users_DeletedByUser]
GO
ALTER TABLE [dbo].[Users]  WITH CHECK ADD  CONSTRAINT [FK_Users_UpdatedByUser] FOREIGN KEY([UpdatedByUserId])
REFERENCES [dbo].[Users] ([Id])
GO
ALTER TABLE [dbo].[Users] CHECK CONSTRAINT [FK_Users_UpdatedByUser]
GO
