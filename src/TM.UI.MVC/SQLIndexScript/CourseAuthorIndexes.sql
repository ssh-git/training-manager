use [TrainingManagerDb]
go

CREATE NONCLUSTERED INDEX [_dta_index_CourseAuthor_7_277576027__K5] ON [Catalog].[CourseAuthor]
(
	[IsDeleted] ASC
)WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]
go


CREATE NONCLUSTERED INDEX [_dta_index_CourseAuthor_7_277576027__K2_K1] ON [Catalog].[CourseAuthor]
(
	[AuthorId] ASC,
	[CourseId] ASC
)WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]
go