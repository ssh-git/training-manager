use [TrainingManagerDb]
go

CREATE NONCLUSTERED INDEX [_dta_index_ToC_Topic_7_501576825__K2_K1_3_4_5] ON [Catalog].[ToC_Topic]
(
	[ModuleId] ASC,
	[Id] ASC
) WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]
go


CREATE NONCLUSTERED INDEX [_dta_index_ToC_Module_7_469576711__K2_K1_3_4_5_6] ON [Catalog].[ToC_Module]
(
	[CourseId] ASC,
	[Id] ASC
) WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]
go