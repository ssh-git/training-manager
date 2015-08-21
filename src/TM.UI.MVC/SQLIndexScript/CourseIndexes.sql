use [TrainingManagerDb]
go

CREATE NONCLUSTERED INDEX [_dta_index_Course_7_309576141__K1_K3_K17] ON [Catalog].[Course]
(
	[Id] ASC,
	[TrainingProviderId] ASC,
	[IsDeleted] ASC
)WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]
go

CREATE NONCLUSTERED INDEX [_dta_index_Course_7_309576141__K17_K1] ON [Catalog].[Course]
(
	[IsDeleted] ASC,
	[Id] ASC
)WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]
go

CREATE NONCLUSTERED INDEX [_dta_index_Course_7_309576141__K4_K17] ON [Catalog].[Course]
(
	[CategoryId] ASC,
	[IsDeleted] ASC
)WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]
go


CREATE NONCLUSTERED INDEX [_dta_index_Course_7_309576141__K15D_1_3_4_5_7_12_17] ON [Catalog].[Course]
(
	[ReleaseDate] DESC
)
INCLUDE ( 	[Id],
	[TrainingProviderId],
	[CategoryId],
	[Title],
	[UrlName],
	[Rating_Raters],
	[IsDeleted]) WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]
go


CREATE NONCLUSTERED INDEX [_dta_index_Course_7_309576141__K12D_1_4_5_7] ON [Catalog].[Course]
(
	[Rating_Raters] DESC
)
INCLUDE ( 	[Id],
	[CategoryId],
	[Title],
	[UrlName]) WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]
go


CREATE NONCLUSTERED INDEX [_dta_index_Course_7_309576141__K5_K17] ON [Catalog].[Course]
(
	[Title] ASC,
	[IsDeleted] ASC
)WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]
go

CREATE NONCLUSTERED INDEX [_dta_index_Course_7_309576141__K3_K17_K1_K4] ON [Catalog].[Course]
(
	[TrainingProviderId] ASC,
	[IsDeleted] ASC,
	[Id] ASC,
	[CategoryId] ASC
)WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]
go