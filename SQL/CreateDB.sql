drop view vSeason;

drop table AttendenceOption;
drop table Person_PersonRole;
drop table PersonRole;
drop table Person_Event;
drop table Person_Season_Instrument;
drop table Person_Orchestra;
drop table Person;
drop table Instrument;
drop table InstrumentRegister;
drop table InstrumentGroup;
drop table [Event];
drop table EventType;
drop table Season;
drop table Orchestra;
go

create table Orchestra(
    ID int not null identity(1,1) primary key,
    UniqueID uniqueidentifier not null default newid(),
    Name varchar(100) not null,
    IsActive bit not null default 1,
    Comment varchar(max)
);
go

create table Season(
    ID int not null identity(1,1) primary key,
    OrchestraID int not null references Orchestra(ID),
    Caption varchar(100) not null,
    IsActive bit not null default 1,
    Comment varchar(max)
);
go

create table EventType(
    ID int not null identity(1,1) primary key,
    OrchestraID int not null references Orchestra(ID),
    Name varchar(50) null,
    Parameters nvarchar(max) null,
    IsActive bit not null default 1,
    Comment varchar(max)
);
go

create table [Event](
    ID int not null identity(1,1) primary key,
    EventTypeID int not null references EventType(ID),
    SeasonID int not null references Season(ID),
    DateAt date not null,
    LocationAt varchar(1000) null,
    IsActive bit not null default 1,
    Comment varchar(max)
);
go

create table InstrumentGroup(
    ID int not null identity(1,1) primary key,
    OrchestraID int not null references Orchestra(ID),
    Name varchar(50) null,
    [Order] int not null default 0,
    IsActive bit not null default 1,
    Comment varchar(max)
);
go

create table InstrumentRegister(
    ID int not null identity(1,1) primary key,
    InstrumentGroupID int not null references InstrumentGroup(ID),
    Name varchar(50) null,
    [Order] int not null default 0,
    IsActive bit not null default 1,
    Comment varchar(max)
);
go

create table Instrument(
    ID int not null identity(1,1) primary key,
    InstrumentRegisterID int not null references InstrumentRegister(ID),
    Name varchar(50) not null,
    [Order] int not null default 0,
    IsActive bit not null default 1,
    Comment varchar(max)
);
go

create table Person(
    ID int not null identity(1,1) primary key,
    UniqueID uniqueidentifier not null default newid(),
    FirstName varchar(100) null,
    LastName varchar(100) null,
    ContactData nvarchar(max) null,
    IsActive bit not null default 1,
    Comment varchar(max)
);
go

create table Person_Orchestra(
    ID int not null identity(1,1) primary key,
    PersonID int not null references Person(ID),
    OrchestraID int not null references Orchestra(ID),
    PersonalizedGUID uniqueidentifier not null default newid(),
    IsActive bit not null default 1,
    Comment varchar(max)
);
go

create table Person_Season_Instrument(
    ID int not null identity(1,1) primary key,
    PersonID int not null references Person(ID),
    SeasonID int not null references Season(ID),
    InstrumentID int not null references Instrument(ID),
    IsActive bit not null default 1,
    Comment varchar(max)
);
go

create table Person_Event(
    ID int not null identity(1,1) primary key,
    PersonID int not null references Person(ID),
    EventID int not null references [Event](ID),
    IsPresent int not null default 0,
    IsActive bit not null default 1,
    Comment varchar(max)
);
go

create table PersonRole(
    ID int not null identity(1,1) primary key,
    OrchestraID int not null references Orchestra(ID),
    Name varchar(50) null,
    Parameters nvarchar(max) null,
    IsActive bit not null default 1,
    Comment varchar(max)
);
go

create table Person_PersonRole(
    ID int not null identity(1,1) primary key,
    PersonID int not null references Person(ID),
    PersonRoleID int not null references PersonRole(ID),
    IsActive bit not null default 1,
    Comment varchar(max)
);
go

create table AttendenceOption (
    ID int not null identity(1,1) primary key,
    AltText varchar(max) null,
    Value varchar(50) null,
    OrchestraID int not null references Orchestra(ID),
    ColorLight char(6),
    ColorDark char(6),
    SymbolName varchar(50),
    IsActive bit not null default 1,
    IsMandatory bit not null default 1,
    [Order] int not null default 0,
    Comment varchar(max)
);

create view vSeason
as
select  s.ID,
        s.OrchestraID,
        s.Caption,
        s.Comment,
        s.IsActive,
        StartDate = min(e.DateAt),
        EndDate = max(e.DateAt),
        IsCurrent = case when getdate() between min(e.DateAt) and max(e.DateAt) then 1 else 0 end
from    Season s
left    join    [Event] e
    on  e.SeasonID = s.ID
group   by  s.ID,
            s.OrchestraID,
            s.Caption,
            s.Comment,
            s.IsActive