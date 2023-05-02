/*
This query prepares a list of data object mappings based on a SQL server catalog.
The resulting JSON file can be imported in TEAM to create a list of mappings that are essentially one-to-one without having to type them in.
*/

SELECT 
 1 AS enabledIndicator
,HASHBYTES('MD5',src.TABLE_SCHEMA+'|'+src.TABLE_CATALOG+'|'+src.TABLE_NAME) AS tableMappingHash
,src.TABLE_NAME AS sourceTable
,'StagingConnectionInternalId' AS sourceConnectionKey
,'STG_<system>_'+src.TABLE_NAME as targetTable
,'PsaConnectionInternalId' as targetConnectionKey
,COALESCE(pk.KEY_COLUMNS,'') as businessKeyDefinition
,'' AS filterCriteria
FROM INFORMATION_SCHEMA.TABLES src
LEFT JOIN 
(
       SELECT 
        CONSTRAINT_SCHEMA
       ,TABLE_NAME
       --,CHARINDEX(',',KEY_COLUMNS)
       ,CASE 
              WHEN CHARINDEX(';',KEY_COLUMNS)>0
              THEN 'COMPOSITE('+KEY_COLUMNS+')'
              ELSE KEY_COLUMNS
       END AS KEY_COLUMNS
       FROM
       (
       SELECT 
              CONSTRAINT_SCHEMA,
              TABLE_NAME,
              STUFF
              (
                     (
                           SELECT ';' + US.COLUMN_NAME 
                           FROM
						    (
								SELECT 
								   sch.name AS [CONSTRAINT_SCHEMA]
								  ,tbl.name AS [TABLE_NAME]
								  ,COL_NAME(ixc.object_id,ixc.column_id) AS [COLUMN_NAME] 
							    FROM sys.tables tbl
								LEFT OUTER JOIN sys.index_columns ixc ON tbl.object_id = ixc.object_id
								LEFT OUTER JOIN sys.indexes idx ON tbl.object_id = idx.object_id AND ixc.index_id = idx.index_id
								LEFT OUTER JOIN sys.schemas sch ON tbl.schema_id = sch.schema_id
								WHERE idx.is_unique=1
							) US
                           WHERE US.CONSTRAINT_SCHEMA = SS.CONSTRAINT_SCHEMA
                           AND US.TABLE_NAME = SS.TABLE_NAME
                           FOR XML PATH('')             
                       ), 1, 1, ''
              ) KEY_COLUMNS
       FROM 
	   (
			SELECT 
				sch.name AS [CONSTRAINT_SCHEMA]
				,tbl.name AS [TABLE_NAME]
				,COL_NAME(ixc.object_id,ixc.column_id) AS [COLUMN_NAME] 
				,idx.*
			FROM sys.tables tbl
			LEFT OUTER JOIN sys.index_columns ixc ON tbl.object_id = ixc.object_id
			LEFT OUTER JOIN sys.indexes idx ON tbl.object_id = idx.object_id AND ixc.index_id = idx.index_id
			LEFT OUTER JOIN sys.schemas sch ON tbl.schema_id = sch.schema_id
			WHERE idx.is_unique=1
	   ) SS
       --WHERE CONSTRAINT_SCHEMA='dbo'
       GROUP BY CONSTRAINT_SCHEMA, TABLE_NAME
       ) sub
)pk 
ON src.TABLE_NAME = pk.TABLE_NAME
AND src.TABLE_SCHEMA = pk.CONSTRAINT_SCHEMA
--WHERE src.TABLE_NAME IN
--(
--'<ADD YOUR LIST HERE>'
--)
--FOR JSON PATH