use [TrainingManagerDb]
go

CREATE NONCLUSTERED INDEX [_dta_index_TrainingProviderAuthor_7_405576483__K1_K2_3_5_6] ON [Catalog].[TrainingProviderAuthor]
(
	[TrainingProviderId] ASC,
	[AuthorId] ASC
)
INCLUDE ( 	[FullName],
	[UrlName],
	[IsDeleted]) WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]
go


CREATE NONCLUSTERED INDEX [_dta_index_TrainingProviderAuthor_7_405576483__K1_K2_3_5] ON [Catalog].[TrainingProviderAuthor]
(
	[TrainingProviderId] ASC,
	[AuthorId] ASC
)
INCLUDE ( 	[FullName],
	[UrlName]) WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]
go