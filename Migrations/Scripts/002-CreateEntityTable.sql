CREATE TABLE Entity
(
	EntityID int not null identity primary key,
	ChildEntityID int null,
	[Description] varchar(50) null,
	CONSTRAINT FK_Entity_ChildEntity FOREIGN KEY (ChildEntityID)     
    REFERENCES ChildEntity (ChildEntityID)
)