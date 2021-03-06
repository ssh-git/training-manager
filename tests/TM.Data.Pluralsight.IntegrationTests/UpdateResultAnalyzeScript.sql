

--AUTHORS UPDATE ANALYZE
Print 'AUTHORS UPDATE ANALYZE'
SELECT [trainingProvider].[Name] AS [Provider]
		, [author].[AuthorId] AS [Author Id]
		, [author].[FullName]
		, CASE [author].[IsDeleted] WHEN 0 THEN 'NO' ELSE 'YES' END AS [Is Deleted]
		, [operationType].[Name] AS [Update Action]
		, [backup].[FullName] AS [Bkp FullName] 
		, [backup].[SiteUrl] AS [Bkp Url]
  FROM [Catalog].[TrainingProviderAuthor] AS [author]
  JOIN [Catalog].[TrainingProvider] AS [trainingProvider]
	ON [author].[TrainingProviderId] = [trainingProvider].Id
  LEFT JOIN [Update].[AuthorUpdate] AS [authorUpdate]
	ON [authorUpdate].[TrainingProviderId] = [author].[TrainingProviderId] 
		AND [authorUpdate].[AuthorId] = [author].[AuthorId]
  LEFT JOIN [Enum].[OperationType] AS [operationType]
	ON [authorUpdate].[OperationType] = [operationType].[Id]  
  LEFT JOIN [Update].[AuthorBackup] AS [backup]
	ON [authorUpdate].[AuthorId] = [backup].AuthorId 
		AND [authorUpdate].[TrainingProviderId] = [backup].[TrainingProviderId] 
		AND [authorUpdate].[UpdateEventId] = [backup].[UpdateEventId]


--CATEGORIES UPDATE ANALYZE
Print 'CATEGORIES UPDATE ANALYZE'
SELECT [trainingProvider].[Name] AS [Provider]
		, [category].[Id] AS [Category Id]
		, [category].[Title]
		, CASE [category].[IsDeleted] WHEN 0 THEN 'NO' ELSE 'YES' END AS [Is Deleted]
		, [operationType].[Name] AS [Update Action]
		, [backup].[Title] AS [Bkp Title]
		, [backup].[LogoUrl] AS [Bkp LogoUrl]
		, [backup].[LogoFileName] AS [Bkp LogoFileName]
  FROM [Catalog].[Category] AS [category]
  JOIN [Catalog].[TrainingProvider] AS [trainingProvider]
	ON [category].[TrainingProviderId] = [trainingProvider].Id
  LEFT JOIN [Update].[CategoryUpdate] AS [categoryUpdate]
	ON [categoryUpdate].[CategoryId] = [category].[Id]
  LEFT JOIN [Enum].[OperationType] AS [operationType]
	ON [categoryUpdate].[OperationType] = [operationType].Id  
  LEFT JOIN [Update].[CategoryBackup] AS [backup]
	ON [categoryUpdate].[CategoryId] = [backup].[CategoryId] AND [categoryUpdate].[UpdateEventId] = [backup].[UpdateEventId]


--COURSES UPDATE ANALYZE
Print 'COURSES UPDATE ANALYZE'
SELECT	[trainingProvider].[Name] AS [Provider]
		, [category].[Title] AS [Category]
		, [course].[Id] AS [Id]
		, [course].[Title] AS [Title]
		, CASE [course].[IsDeleted] WHEN 0 THEN 'NO' ELSE 'YES' END AS [Is Deleted]
		, [operation].[Name] AS [Update Action]
		, [backup].[Bkp Category]
	    , [backup].[Title] as [Bkp Title]
	    , [backup].[SiteUrl] as [Bkp Url]
	    , [backup].[Description] as [Bkp Descr]
	    , CASE WHEN [backup].[HasClosedCaptions] IS NOT NULL THEN (CASE [backup].[HasClosedCaptions] WHEN 0 THEN 'NO' ELSE 'YES' END) END as [Bkp Has Subtitles]
	    , [backup].[Bkp Level]	    
	    , [backup].[Duration] as [Bkp Duration]
	    , [backup].[ReleaseDate] as [Bkp ReleaseDate]								 
	FROM [Catalog].[Course] as [course]
	JOIN [Catalog].[TrainingProvider] AS [trainingProvider]
		ON [course].[TrainingProviderId] = [trainingProvider].[Id]
	LEFT JOIN [Catalog].[Category] as [category]
		ON [course].[CategoryId] = [category].[id]
	LEFT JOIN [Update].[CourseUpdate] AS [courseUpdate]
		ON [courseUpdate].[CourseId] = [course].[Id]
	LEFT JOIN [Enum].[OperationType] AS [operation]
		ON [courseUpdate].[OperationType] = [operation].[Id] 
	LEFT JOIN 
		(SELECT [cat].[Title] As [Bkp Category]
				, [courseLevel].[Name] AS [Bkp Level]
			    , [bkp].*
			FROM [Update].[CourseBackup] AS [bkp] 
			LEFT JOIN [Catalog].[Category] AS [cat] 
				ON [bkp].[CategoryId] = [cat].[Id]
			LEFT JOIN [Enum].[CourseLevel] AS [courseLevel]
				ON [bkp].[CourseLevel] = [courseLevel].[Id]			
		 ) AS [backup]
		ON [backup].[CourseId] = [courseUpdate].[CourseId]	AND [backup].[UpdateEventId] = [courseUpdate].[UpdateEventId]
	ORDER BY [category].[Title]


--COURSE AUTHORS UPDATE ANALYZE
Print 'COURSE AUTHORS UPDATE ANALYZE'
SELECT	[trainingProvider].[Name] AS [Provider]
		, [category].[Title] AS [Category]
		, [course].[Id] AS [Course Id]
		, [course].[Title] AS [Title]
		, CASE [course].[IsDeleted] WHEN 0 THEN 'NO' ELSE 'YES' END AS [Is Deleted]
		, [operation].[Name] AS [Update Action]
		, [authorsBackup].FullName
		, [authorsBackup].UrlName
		, [authorsBackup].IsCoAuthor
		, [authorsBackup].Operation		
						 
	FROM [Catalog].[Course] as [course]
	JOIN [Catalog].[TrainingProvider] AS [trainingProvider]
		ON [course].[TrainingProviderId] = [trainingProvider].[Id]
	LEFT JOIN [Catalog].[Category] as [category]
		ON [course].[CategoryId] = [category].[id]
	LEFT JOIN [Update].[CourseUpdate] AS [courseUpdate]
		ON [courseUpdate].[CourseId] = [course].[Id]
	LEFT JOIN [Enum].[OperationType] AS [operation]
		ON [courseUpdate].[OperationType] = [operation].[Id] 
	LEFT JOIN
	 (SELECT [ca_bak].[CourseId] AS [CourseId]
			, [ca_bak].[UpdateEventId] AS [UpdateEventId]
			, CASE WHEN [ca_bak].[IsAuthorCoAuthor] IS NOT NULL THEN (CASE [ca_bak].[IsAuthorCoAuthor] WHEN 0 THEN 'NO' ELSE 'YES' END) END AS [IsCoAuthor]
			, [ot].[Name] AS [Operation]
			, [tp_auth].[FullName] AS [FullName]
			, [tp_auth].[UrlName] AS [UrlName]
	 	FROM [Update].[CourseAuthorBackup] As [ca_bak]
		LEFT JOIN [Catalog].[TrainingProviderAuthor] [tp_auth]
			ON [ca_bak].[AuthorId] = [tp_auth].[AuthorId] AND [ca_bak].[TrainingProviderId] = [tp_auth].[TrainingProviderId]
		JOIN [Enum].[OperationType] AS [ot]
			ON [ca_bak].[OperationType] = [ot].[Id]			
	 )AS [authorsBackup]	
		ON [course].[Id] = [authorsBackup].[CourseId] AND [courseUpdate].[UpdateEventId] =  [authorsBackup].[UpdateEventId]
		Where NOT [authorsBackup].[UpdateEventId] IS NULL		
	ORDER BY [category].[Title]


--AUTHORS RESOLVE LIST ANALYZE
Print 'AUTHORS RESOLVE LIST ANALYZE'

SELECT	[trainingProvider].[Name] AS [Provider]
		, [category].[Title] AS [Category]
		, [course].[Id] AS [Course Id]
		, [course].[Title] AS [Title]
		, CASE [course].[IsDeleted] WHEN 0 THEN 'NO' ELSE 'YES' END AS [Is Deleted]
		, [resolve].[AuthorFullName] AS [Full Name]
		, [resolve].[AuthorSiteUrl] AS [Url]
		, [resolve].[AuthorUrlName] AS [Url Name]
		, CASE [resolve].[IsAuthorCoAuthor] WHEN 0 THEN 'NO' ELSE 'YES' END AS [Is CoAuthor]
		, [resolveState].Name AS [Resolve State]
		, [problemType].[Name] AS [Problem Type]		
						 
	FROM [Update].[AuthorResolve] as [resolve]
	JOIN [Catalog].[TrainingProvider] AS [trainingProvider]
		ON [resolve].[TrainingProviderId] = [trainingProvider].[Id]
	JOIN [Catalog].[Course] as [course]
		on [resolve].[CourseId] = [course].[Id]
	JOIN [Catalog].[Category] as [category]
		ON [course].[CategoryId] = [category].[id]
	JOIN [Enum].[ResolveState] AS [resolveState]
		ON [resolve].[ResolveState] = [resolveState].[Id]
	JOIN [Enum].[ProblemType] AS [problemType]
		ON [resolve].[ProblemType] = [problemType].[Id]	