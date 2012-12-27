CREATE TABLE [dbo].[LincatProductsTable] (
    [Id]          INT            NOT NULL IDENTITY,
    [Model]       NVARCHAR (100) NOT NULL,
    [Range]       NVARCHAR (100) NOT NULL,
    [Description] NVARCHAR (250) NULL,
    [Height]      INT            NULL,
    [Width]       INT            NULL,
    [Depth]       INT            NULL,
    [Power]       NVARCHAR (50)  NULL,
    [Fuel]        NVARCHAR (50)  NULL,
    [Notes]       NVARCHAR (MAX) NULL,
    [Category]    NVARCHAR (50)  NULL,
    [SubCategory] NVARCHAR (50)  NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);

