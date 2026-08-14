using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Wheeeee.Calendar37.Core.Enums;
using Wheeeee.Calendar37.Core.Interfaces;
using Wheeeee.Calendar37.Repositories.Interfaces;
using Wheeeee.Calendar37.Repositories.Model;
using Wheeeee.Core.Extensions;
using Wheeeee.Core.Interfaces.Collections;
using Wheeeee.Repositories;
using Wheeeee.Repositories.Extensions;

namespace Wheeeee.Calendar37.Repositories
{
    public class CalenderRepository : Repository, ICalenderRepository
    {
        public CalenderRepository(string connectionString)
            : base(connectionString)
        { }

        public IEnumerable<IMembership> GetMembershipsByUniqueIDs(IEnumerable<Guid> personUniqueIDs)
        {
            string sql = $@"
declare @ParsedIDs table
(
    PersonUniqueID uniqueidentifier primary key
);

insert into @ParsedIDs(PersonUniqueID)
select distinct try_convert(uniqueidentifier, ltrim(rtrim(value)))
from string_split(@PersonUniqueIDsCsv, ',')
where try_convert(uniqueidentifier, ltrim(rtrim(value))) is not null;

declare @SessionPersons table
(
    SessionPersonID int primary key,
    SessionPersonUniqueID uniqueidentifier not null,
    FirstName varchar(100) null,
    LastName varchar(100) null
);

insert into @SessionPersons(SessionPersonID, SessionPersonUniqueID, FirstName, LastName)
select
    p.ID,
    p.UniqueID,
    p.FirstName,
    p.LastName
from Person p
join @ParsedIDs x
  on x.PersonUniqueID = p.UniqueID
where p.IsActive = 1;

declare @RoleRows table
(
    SessionPersonID int not null,
    SessionPersonUniqueID uniqueidentifier not null,
    OrchestraID int not null,
    OrchestraUniqueID uniqueidentifier not null,
    OrchestraName varchar(100) not null,
    RoleID int not null,
    RoleName varchar(50) null,
    CanEditOthers varchar(20) not null
);

insert into @RoleRows
(
    SessionPersonID, SessionPersonUniqueID,
    OrchestraID, OrchestraUniqueID, OrchestraName,
    RoleID, RoleName, CanEditOthers
)
select
    sp.SessionPersonID,
    sp.SessionPersonUniqueID,
    o.ID,
    o.UniqueID,
    o.Name,
    r.ID,
    r.Name,
    isnull(json_value(r.Parameters, '$.canEditOthers'), 'no')
from @SessionPersons sp
join Person_PersonRole ppr
  on ppr.PersonID = sp.SessionPersonID
 and ppr.IsActive = 1
join PersonRole r
  on r.ID = ppr.PersonRoleID
 and r.IsActive = 1
join Orchestra o
  on o.ID = r.OrchestraID
 and o.IsActive = 1;

declare @RoleAgg table
(
    SessionPersonID int not null,
    SessionPersonUniqueID uniqueidentifier not null,
    OrchestraID int not null,
    CanEditRank int not null,
    IsAdmin bit not null,
    Roles varchar(max) null,
    primary key (SessionPersonID, OrchestraID)
);

insert into @RoleAgg
(
    SessionPersonID, SessionPersonUniqueID, OrchestraID,
    CanEditRank, IsAdmin, Roles
)
select
    rr.SessionPersonID,
    rr.SessionPersonUniqueID,
    rr.OrchestraID,
    min(case rr.CanEditOthers
            when 'all' then 1
            when 'register' then 2
            when 'group' then 3
            else 4
        end) as CanEditRank,
    convert(bit, max(case when rr.RoleName = 'Admin' then 1 else 0 end)) as IsAdmin,
    (
        select string_agg(x.s, '|')
        from (
            select distinct convert(varchar(max), rr2.RoleID) as s
            from @RoleRows rr2
            where rr2.SessionPersonID = rr.SessionPersonID
              and rr2.OrchestraID = rr.OrchestraID
        ) x
    ) as Roles
from @RoleRows rr
group by rr.SessionPersonID, rr.SessionPersonUniqueID, rr.OrchestraID;

declare @Playing table
(
    SessionPersonID int not null,
    SessionPersonUniqueID uniqueidentifier not null,
    OrchestraID int not null,
    OrchestraUniqueID uniqueidentifier not null,
    OrchestraName varchar(100) not null,
    SeasonID int not null,
    SeasonCaption varchar(100) null,
    primary key (SessionPersonID, OrchestraID, SeasonID)
);

insert into @Playing
(
    SessionPersonID, SessionPersonUniqueID,
    OrchestraID, OrchestraUniqueID, OrchestraName,
    SeasonID, SeasonCaption
)
select distinct
    sp.SessionPersonID,
    sp.SessionPersonUniqueID,
    po.OrchestraID,
    o.UniqueID,
    o.Name,
    s.ID,
    s.Caption
from @SessionPersons sp
join Person_Orchestra po
  on po.PersonID = sp.SessionPersonID
 and po.IsActive = 1
join Orchestra o
  on o.ID = po.OrchestraID
 and o.IsActive = 1
join vSeason s
  on s.OrchestraID = po.OrchestraID
 and s.IsCurrent = 1
 and s.IsActive = 1
where exists
(
    select 1
    from Person_Season_Instrument psi
    where psi.PersonID = sp.SessionPersonID
      and psi.SeasonID = s.ID
      and psi.IsActive = 1
);

declare @InstrumentAgg table
(
    SessionPersonID int not null,
    OrchestraID int not null,
    Instruments varchar(max) null,
    primary key (SessionPersonID, OrchestraID)
);

insert into @InstrumentAgg(SessionPersonID, OrchestraID, Instruments)
select
    p.SessionPersonID,
    p.OrchestraID,
    (
        select string_agg(x.s, '|')
        from (
            select distinct convert(varchar(max), i.ID) as s
            from Person_Season_Instrument psi
            join vSeason s2
              on s2.ID = psi.SeasonID
             and s2.IsCurrent = 1
             and s2.IsActive = 1
            join Instrument i
              on i.ID = psi.InstrumentID
             and i.IsActive = 1
            where psi.PersonID = p.SessionPersonID
              and psi.IsActive = 1
              and s2.OrchestraID = p.OrchestraID
        ) x
    ) as Instruments
from @Playing p
group by p.SessionPersonID, p.OrchestraID;

declare @Universe table
(
    SessionPersonID int not null,
    SessionPersonUniqueID uniqueidentifier not null,
    OrchestraID int not null,
    OrchestraUniqueID uniqueidentifier not null,
    OrchestraName varchar(100) not null,
    primary key (SessionPersonID, OrchestraID)
);

insert into @Universe
(
    SessionPersonID, SessionPersonUniqueID,
    OrchestraID, OrchestraUniqueID, OrchestraName
)
select distinct
    p.SessionPersonID,
    p.SessionPersonUniqueID,
    p.OrchestraID,
    p.OrchestraUniqueID,
    p.OrchestraName
from @Playing p;

insert into @Universe
(
    SessionPersonID, SessionPersonUniqueID,
    OrchestraID, OrchestraUniqueID, OrchestraName
)
select
    rr.SessionPersonID,
    rr.SessionPersonUniqueID,
    rr.OrchestraID,
    rr.OrchestraUniqueID,
    rr.OrchestraName
from
(
    select distinct
        SessionPersonID, SessionPersonUniqueID,
        OrchestraID, OrchestraUniqueID, OrchestraName
    from @RoleRows
) rr
where not exists
(
    select 1
    from @Universe u
    where u.SessionPersonID = rr.SessionPersonID
      and u.OrchestraID = rr.OrchestraID
);

declare @Context table
(
    PersonID int not null,
    OrchestraID int not null,
    SeasonID int null,
    InstrumentIDs varchar(max) null,
    RoleIDs varchar(max) null,
    IsAdmin bit not null,
    HasPlayingMembership bit not null,
    EffectiveCanEditOthers varchar(20) not null,
    RehearsalVisible bit not null,
    primary key (PersonID, OrchestraID)
);

insert into @Context
(
    PersonID,
    OrchestraID,
    SeasonID,
    InstrumentIDs,
    RoleIDs,
    IsAdmin,
    HasPlayingMembership,
    EffectiveCanEditOthers,
    RehearsalVisible
)
select
    u.SessionPersonID,
    u.OrchestraID,
    p.SeasonID,
    ia.Instruments,

    ra.Roles,
    isnull(ra.IsAdmin, 0) as IsAdmin,

    convert(bit, case when p.OrchestraID is null then 0 else 1 end) as HasPlayingMembership,

    case isnull(ra.CanEditRank, 4)
        when 1 then 'all'
        when 2 then 'register'
        when 3 then 'group'
        else 'no'
    end as EffectiveCanEditOthers,

    convert(bit, case
        when p.OrchestraID is not null then 1
        when isnull(ra.CanEditRank, 4) = 1 then 1
        else 0
    end) as RehearsalVisible
from @Universe u
left join @Playing p
  on p.SessionPersonID = u.SessionPersonID
 and p.OrchestraID = u.OrchestraID
left join @RoleAgg ra
  on ra.SessionPersonID = u.SessionPersonID
 and ra.OrchestraID = u.OrchestraID
left join @InstrumentAgg ia
  on ia.SessionPersonID = u.SessionPersonID
 and ia.OrchestraID = u.OrchestraID;

--------------------------------------------------------------------------------
-- RESULT SET 1: resolved session persons
--------------------------------------------------------------------------------
select
    ID        =   sp.SessionPersonID,
    UniqueID  =   sp.SessionPersonUniqueID,
    sp.FirstName,
    sp.LastName
from @SessionPersons sp
order by sp.LastName, sp.FirstName, sp.SessionPersonID;

--------------------------------------------------------------------------------
-- RESULT SET 2: per-person orchestra contexts
--------------------------------------------------------------------------------
select
    c.PersonID,
    c.OrchestraID,
    c.SeasonID,
    c.InstrumentIDs,
    c.RoleIDs,
    c.IsAdmin,
    c.HasPlayingMembership,
    c.EffectiveCanEditOthers,
    c.RehearsalVisible
from @Context c

--------------------------------------------------------------------------------
-- RESULT SET 3: instruments per orchestra
--------------------------------------------------------------------------------
select	ID					    =	i.ID,
		Name					=	i.Name,
		[Order]					=	i.[Order],
		RegisterName			=	ir.Name,
		RegisterOrder			=	ir.[Order],
		GroupName				=	ig.Name,
		GroupOrder			    =	ig.[Order],
		ig.OrchestraID
from	Instrument i
join	InstrumentRegister ir
	on	ir.ID = i.InstrumentRegisterID
join	InstrumentGroup ig
	on	ig.ID = ir.InstrumentGroupID
where	i.IsActive = 1
	and	ir.IsActive = 1
	and	ig.IsActive = 1

--------------------------------------------------------------------------------
-- RESULT SET 4: roles per orchestra
--------------------------------------------------------------------------------
select	r.ID,
		r.Name,
        r.OrchestraID
from	PersonRole r
where	r.IsActive = 1

--------------------------------------------------------------------------------
-- RESULT SET 5: attendence options per orchestra
--------------------------------------------------------------------------------
select	ao.ID,
		ao.AltText,
		ao.Value,
		ao.OrchestraID,
		ao.ColorLight,
		ao.ColorDark,
		ao.SymbolName,
		ao.Comment,
		ao.IsMandatory,
		ao.[Order]
from	AttendenceOption ao
join	@Universe u
	on	u.OrchestraID = ao.OrchestraID
where	IsActive = 1
";

            DataSet dataSet = LoadDataSet(sql, ["p", "m", "i", "r", "ao"], new Dictionary<string, object> { { "PersonUniqueIDsCsv", string.Join(",", personUniqueIDs) } });
            DataRow row = dataSet.Tables["p"].Rows.Cast<DataRow>().FirstOrDefault();
            if (row == null)
            {
                return Array.Empty<IMembership>();
            }

            var personData = DataRowExtension.ToDataCollection(row);
            personData.Add("CanEditOthers", dataSet.Tables["p"].Rows.Cast<DataRow>().Select(r => r["CanEditOthers"].To<CanEditOthers>()).Min());
            var person = InstantiatePerson(personData);
            var allInstruments = dataSet.Tables["i"]
                .Rows.Cast<DataRow>()
                .Select(r => InstantiateInstrument(DataRowExtension.ToDataCollection(r)))
                .ToArray();
            var attendenceOptions = dataSet.Tables["ao"]
                .Rows.Cast<DataRow>()
                .Select(r => InstantiateAttendenceOption(DataRowExtension.ToDataCollection(r)))
                .ToArray();
            var memberships = dataSet.Tables["m"]
                .Rows.Cast<DataRow>()
                .Select(r => r.ToDataCollection())
                .Select(r =>
                {
                    var orchestra = InstantiateOrchestra(r, attendenceOptions.Where(ao => ao.OrchestraID == r.Get<int>("OrchestraID")), "Orchestra");


                    var season = new Season(
                        r.Get<int>("SeasonID"),
                        r.Get<string>("SeasonCaption"),
                        r.Get<DateTime?>("StartDate"),
                        r.Get<DateTime?>("EndDate"),
                        orchestra);

                    var rolesDataRows = r.Get<string>("OrchestraRoles")?.Split('|') ?? Array.Empty<string>();
                    var roles = rolesDataRows
                        .Select(s =>
                        {
                            var parts = s.Split(';');
                            return new Role(int.Parse(parts[0]), parts[1], "");
                        })
                        .ToArray();

                    var instrumentsDataRows = r.Get<string>("Instruments")?.Split('|') ?? Array.Empty<string>();
                    var instruments = instrumentsDataRows
                        .Select(s => int.Parse(s.Split(';')[0]))
                        .Select(id => allInstruments.SingleOrDefault(i => i.ID == id))
                        .ToArray();

                    return new Membership(r.Get<Guid>("PersonalizedGUID"), person, orchestra, season, instruments, roles);
                }).ToArray();

            return memberships;
        }

        public IMembership GetMembershipByUniqueID(Guid membershipID)
        {
            var sql = $@"
declare @pID int
declare @orchestraID int

select	distinct 
        @pID = p.ID,
        @orchestraID = o.ID
from	Person p
join	Person_Orchestra po
    on	po.PersonID = p.ID
join	Orchestra o
    on	po.OrchestraID = o.ID
join	Season s
    on	s.OrchestraID = o.ID
join	Person_Season_Instrument psi 
    on	psi.SeasonID = s.ID 
    and psi.PersonID = po.PersonID
where	po.PersonalizedGUID = @PGuid
    and	p.IsActive = 1
    and	po.IsActive = 1
    and	o.IsActive = 1
    and	s.IsCurrent = 1
    and	s.IsActive = 1
    and	psi.IsActive = 1

select	p.ID,
        p.FirstName,
        p.LastName,
        p.UniqueID,
        CanEditOthers = isnull(json_value(r.Parameters, '$.canEditOthers'), 'no')
from	Person p
join	Person_PersonRole pr
    on	pr.PersonID = p.ID
join	PersonRole r
    on	r.ID = pr.PersonRoleID
where	p.ID = @pID
    and r.OrchestraID = @orchestraID
    and	p.IsActive = 1
    and	pr.IsActive = 1
    and	r.IsActive = 1

select	distinct
        PersonalizedGUID    =   po.PersonalizedGUID,    
        OrchestraID			=	o.ID,
        OrchestraName		=	o.Name,
        OrchestraUniqueID	=	o.UniqueID,
        OrchestraColor1     =	o.Color1,
        OrchestraRoles		=	(
        	select	string_agg(x.s, '|')
        	from	(
        		select	distinct s = convert(varchar(max), r.ID) + ';' + r.Name
        		from	PersonRole r
        		join	Person_PersonRole pr
        			on	pr.PersonRoleID = r.ID
        		where	pr.PersonID = po.PersonID
        			and	r.OrchestraID = o.ID
        			and	pr.IsActive = 1
        			and	r.IsActive = 1) x),
        SeasonID			=	s.ID,
        SeasonCaption		=	s.Caption,
        Instruments			=	(
        	select	string_agg(x.s, '|')
        	from	(
        		select	distinct s = convert(varchar(max), i.ID) + ';' + i.Name
        		from	Instrument i
        		join	Person_Season_Instrument psi
        			on	psi.InstrumentID = i.ID
        		where	psi.PersonID = po.PersonID
        			and psi.SeasonID = s.ID
        			and	i.IsActive = 1
        			and	psi.IsActive = 1) x)
from	Person_Orchestra po
join	Orchestra o
    on	po.OrchestraID = o.ID
join	Season s
    on	s.OrchestraID = o.ID
join	Person_Season_Instrument psi 
    on	psi.SeasonID = s.ID 
    and psi.PersonID = @pID
join	Instrument i
    on	i.ID = psi.InstrumentID
where	po.PersonID = @pID
    and po.PersonalizedGUID = @PGuid
    and	po.IsActive = 1
    and	o.IsActive = 1
    and	s.IsCurrent = 1
    and	s.IsActive = 1
    and	psi.IsActive = 1
    and	i.IsActive = 1

select	ID					    =	i.ID,
        Name					=	i.Name,
        [Order]					=	i.[Order],
        RegisterName			=	ir.Name,
        RegisterOrder			=	ir.[Order],
        GroupName				=	ig.Name,
        GroupOrder			    =	ig.[Order],
        ig.OrchestraID
from	Instrument i
join	InstrumentRegister ir
    on	ir.ID = i.InstrumentRegisterID
join	InstrumentGroup ig
    on	ig.ID = ir.InstrumentGroupID
--	and	ig.OrchestraID = @orchestraID
where	i.IsActive = 1
    and	ir.IsActive = 1
    and	ig.IsActive = 1

select	ao.ID,
		ao.AltText,
		ao.Value,
		ao.OrchestraID,
		ao.ColorLight,
		ao.ColorDark,
		ao.SymbolName,
		ao.Comment,
		ao.IsMandatory,
		ao.[Order]
from	AttendenceOption ao
where	ao.IsActive = 1
    and ao.OrchestraID = @orchestraID
";

            DataSet dataSet = LoadDataSet(sql, ["p", "m", "i", "ao"], new Dictionary<string, object> { { "PGuid", membershipID } });
            var persons = dataSet.Tables["p"]
                        .Rows.Cast<DataRow>().Select(r => InstantiatePerson(DataRowExtension.ToDataCollection(r)))
                        .ToArray();
            if (persons.IsNullOrEmpty())
            {
                return null;
            }

            var person = new Person(
                persons[0].ID,
                persons[0].UniqueID,
                persons[0].FirstName,
                persons[0].LastName,
                persons[0].Memberships);
            var allInstruments = dataSet.Tables["i"]
                 .Rows.Cast<DataRow>()
                 .Select(r => InstantiateInstrument(DataRowExtension.ToDataCollection(r)))
                 .ToArray();
            var attendenceOptions = dataSet.Tables["ao"]
                .Rows.Cast<DataRow>()
                .Select(r => InstantiateAttendenceOption(DataRowExtension.ToDataCollection(r)))
                .ToArray();
            var membership = dataSet.Tables["m"]
                .Rows.Cast<DataRow>()
                .Select(r => r.ToDataCollection())
                .Select(r =>
                {
                    var orchestra = InstantiateOrchestra(r, attendenceOptions.Where(ao => ao.OrchestraID == r.Get<int>("OrchestraID")), "Orchestra");

                    var season = new Season(
                        r.Get<int>("SeasonID"),
                        r.Get<string>("SeasonCaption"),
                        null, null,
                        orchestra);

                    var rolesDataRows = r.Get<string>("OrchestraRoles")?.Split('|') ?? Array.Empty<string>();
                    var roles = rolesDataRows
                        .Select(s =>
                        {
                            var parts = s.Split(';');
                            return new Role(int.Parse(parts[0]), parts[1], "");
                        })
                        .ToArray();

                    var instrumentsDataRows = r.Get<string>("Instruments")?.Split('|') ?? Array.Empty<string>();
                    var instruments = allInstruments
                        .Where(i => instrumentsDataRows.Select(s => int.Parse(s.Split(';')[0])).Contains(i.ID))
                        .ToArray();

                    return new Membership(r.Get<Guid>("PersonalizedGUID"), person, orchestra, season, instruments, roles);
                }).FirstOrDefault();

            return membership;
        }

        public IPerson GetPersonByUniqueID(string uniqueID)
        {
            const string sql = @"
        select  t.ID,
                t.UniqueID,
                t.FirstName,
                t.LastName,
                os = string_agg(os, '|')
        from   (
                select  p.ID,
                        p.UniqueID,
                        p.FirstName,
                        p.LastName,
                        os = convert(varchar(max), o.ID) + ';' + convert(varchar(max), o.UniqueID) + ';' + o.Name
                from    Person p(nolock)
                join    Person_PersonRole ppr(nolock)
                    on  ppr.PersonID = p.ID
                join    PersonRole pr(nolock)
                    on  pr.ID = ppr.PersonRoleID
                join    Orchestra o(nolock)
                    on  o.ID = pr.OrchestraID
                where   p.UniqueID = @UniqueID
                    and p.IsActive = 1
                    and ppr.IsActive = 1
                    and pr.IsActive = 1
                    and o.IsActive = 1
                ) t
        group   by  t.ID,
                    t.UniqueID,
                    t.FirstName,
                    t.LastName
        ";

            return Load(sql, d => InstantiatePerson(d), new Dictionary<string, object> { { "UniqueID", uniqueID } }).SingleOrDefault();
        }

        public IEnumerable<IPerson> GetPersonInfo(string personUniqueIDs)
        {
            const string sql = @"
declare @SessionPersons table
(
    SessionPersonID int primary key,
    SessionPersonUniqueID uniqueidentifier not null,
    FirstName varchar(100) null,
    LastName varchar(100) null
);

insert into @SessionPersons(SessionPersonID, SessionPersonUniqueID, FirstName, LastName)
select
    p.ID,
    p.UniqueID,
    p.FirstName,
    p.LastName
from Person p
where p.UniqueID = @PersonUniqueID
  and p.IsActive = 1;

declare @RoleRows table
(
    SessionPersonID int not null,
    SessionPersonUniqueID uniqueidentifier not null,
    OrchestraID int not null,
    OrchestraUniqueID uniqueidentifier not null,
    OrchestraName varchar(100) not null,
    RoleID int not null,
    RoleName varchar(50) null,
    CanEditOthers varchar(20) not null
);

insert into @RoleRows
(
    SessionPersonID, SessionPersonUniqueID,
    OrchestraID, OrchestraUniqueID, OrchestraName,
    RoleID, RoleName, CanEditOthers
)
select
    sp.SessionPersonID,
    sp.SessionPersonUniqueID,
    o.ID,
    o.UniqueID,
    o.Name,
    r.ID,
    r.Name,
    isnull(json_value(r.Parameters, '$.canEditOthers'), 'no')
from @SessionPersons sp
join Person_PersonRole ppr
  on ppr.PersonID = sp.SessionPersonID
 and ppr.IsActive = 1
join PersonRole r
  on r.ID = ppr.PersonRoleID
 and r.IsActive = 1
join Orchestra o
  on o.ID = r.OrchestraID
 and o.IsActive = 1;

declare @RoleAgg table
(
    SessionPersonID int not null,
    SessionPersonUniqueID uniqueidentifier not null,
    OrchestraID int not null,
    CanEditRank int not null,
    IsAdmin bit not null,
    Roles varchar(max) null,
    primary key (SessionPersonID, OrchestraID)
);

insert into @RoleAgg
(
    SessionPersonID, SessionPersonUniqueID, OrchestraID,
    CanEditRank, IsAdmin, Roles
)
select
    rr.SessionPersonID,
    rr.SessionPersonUniqueID,
    rr.OrchestraID,
    min(case rr.CanEditOthers
            when 'all' then 1
            when 'register' then 2
            when 'group' then 3
            else 4
        end) as CanEditRank,
    convert(bit, max(case when rr.RoleName = 'Admin' then 1 else 0 end)) as IsAdmin,
    (
        select string_agg(x.s, '|')
        from (
            select distinct convert(varchar(max), rr2.RoleID) as s
            from @RoleRows rr2
            where rr2.SessionPersonID = rr.SessionPersonID
              and rr2.OrchestraID = rr.OrchestraID
        ) x
    ) as Roles
from @RoleRows rr
group by rr.SessionPersonID, rr.SessionPersonUniqueID, rr.OrchestraID;

declare @Playing table
(
    SessionPersonID int not null,
    SessionPersonUniqueID uniqueidentifier not null,
    OrchestraID int not null,
    OrchestraUniqueID uniqueidentifier not null,
    OrchestraName varchar(100) not null,
    SeasonID int not null,
    SeasonCaption varchar(100) null,
    primary key (SessionPersonID, OrchestraID, SeasonID)
);

insert into @Playing
(
    SessionPersonID, SessionPersonUniqueID,
    OrchestraID, OrchestraUniqueID, OrchestraName,
    SeasonID, SeasonCaption
)
select distinct
    sp.SessionPersonID,
    sp.SessionPersonUniqueID,
    po.OrchestraID,
    o.UniqueID,
    o.Name,
    s.ID,
    s.Caption
from @SessionPersons sp
join Person_Orchestra po
  on po.PersonID = sp.SessionPersonID
 and po.IsActive = 1
join Orchestra o
  on o.ID = po.OrchestraID
 and o.IsActive = 1
join vSeason s
  on s.OrchestraID = po.OrchestraID
 and s.IsCurrent = 1
 and s.IsActive = 1
where exists
(
    select 1
    from Person_Season_Instrument psi
    where psi.PersonID = sp.SessionPersonID
      and psi.SeasonID = s.ID
      and psi.IsActive = 1
);

declare @InstrumentAgg table
(
    SessionPersonID int not null,
    OrchestraID int not null,
    Instruments varchar(max) null,
    primary key (SessionPersonID, OrchestraID)
);

insert into @InstrumentAgg(SessionPersonID, OrchestraID, Instruments)
select
    p.SessionPersonID,
    p.OrchestraID,
    (
        select string_agg(x.s, '|')
        from (
            select distinct convert(varchar(max), i.ID) as s
            from Person_Season_Instrument psi
            join vSeason s2
              on s2.ID = psi.SeasonID
             and s2.IsCurrent = 1
             and s2.IsActive = 1
            join Instrument i
              on i.ID = psi.InstrumentID
             and i.IsActive = 1
            where psi.PersonID = p.SessionPersonID
              and psi.IsActive = 1
              and s2.OrchestraID = p.OrchestraID
        ) x
    ) as Instruments
from @Playing p
group by p.SessionPersonID, p.OrchestraID;

declare @Universe table
(
    SessionPersonID int not null,
    SessionPersonUniqueID uniqueidentifier not null,
    OrchestraID int not null,
    OrchestraUniqueID uniqueidentifier not null,
    OrchestraName varchar(100) not null,
    primary key (SessionPersonID, OrchestraID)
);

insert into @Universe
(
    SessionPersonID, SessionPersonUniqueID,
    OrchestraID, OrchestraUniqueID, OrchestraName
)
select distinct
    p.SessionPersonID,
    p.SessionPersonUniqueID,
    p.OrchestraID,
    p.OrchestraUniqueID,
    p.OrchestraName
from @Playing p;

insert into @Universe
(
    SessionPersonID, SessionPersonUniqueID,
    OrchestraID, OrchestraUniqueID, OrchestraName
)
select
    rr.SessionPersonID,
    rr.SessionPersonUniqueID,
    rr.OrchestraID,
    rr.OrchestraUniqueID,
    rr.OrchestraName
from
(
    select distinct
        SessionPersonID, SessionPersonUniqueID,
        OrchestraID, OrchestraUniqueID, OrchestraName
    from @RoleRows
) rr
where not exists
(
    select 1
    from @Universe u
    where u.SessionPersonID = rr.SessionPersonID
      and u.OrchestraID = rr.OrchestraID
);

declare @Context table
(
    PersonID int not null,
    OrchestraID int not null,
    SeasonID int null,
    InstrumentIDs varchar(max) null,
    RoleIDs varchar(max) null,
    IsAdmin bit not null,
    HasPlayingMembership bit not null,
    MembershipUniqueID uniqueidentifier null,
    EffectiveCanEditOthers varchar(20) not null,
    RehearsalVisible bit not null,
    primary key (PersonID, OrchestraID)
);

insert into @Context
(
    PersonID,
    OrchestraID,
    SeasonID,
    InstrumentIDs,
    RoleIDs,
    IsAdmin,
    HasPlayingMembership,
    MembershipUniqueID,
    EffectiveCanEditOthers,
    RehearsalVisible
)
select
    u.SessionPersonID,
    u.OrchestraID,
    p.SeasonID,
    ia.Instruments,
    ra.Roles,
    isnull(ra.IsAdmin, 0) as IsAdmin,
    convert(bit, case when p.OrchestraID is null then 0 else 1 end) as HasPlayingMembership,
    po.PersonalizedGUID as MembershipUniqueID,
    case isnull(ra.CanEditRank, 4)
        when 1 then 'all'
        when 2 then 'register'
        when 3 then 'group'
        else 'no'
    end as EffectiveCanEditOthers,
    convert(bit, case
        when p.OrchestraID is not null then 1
        when isnull(ra.CanEditRank, 4) = 1 then 1
        else 0
    end) as RehearsalVisible
from @Universe u
left join @Playing p
  on p.SessionPersonID = u.SessionPersonID
 and p.OrchestraID = u.OrchestraID
left join @RoleAgg ra
  on ra.SessionPersonID = u.SessionPersonID
 and ra.OrchestraID = u.OrchestraID
left join @InstrumentAgg ia
  on ia.SessionPersonID = u.SessionPersonID
 and ia.OrchestraID = u.OrchestraID
left join Person_Orchestra po
  on po.PersonID = u.SessionPersonID
 and po.OrchestraID = u.OrchestraID
 and po.IsActive = 1;

/* RESULT SET 1 */
select
    ID        = sp.SessionPersonID,
    UniqueID  = sp.SessionPersonUniqueID,
    sp.FirstName,
    sp.LastName
from @SessionPersons sp
order by sp.LastName, sp.FirstName, sp.SessionPersonID;

/* RESULT SET 2 */
select
    c.PersonID,
    c.OrchestraID,
    SeasonID = isnull(c.SeasonID, (select max(ID) from Season s where s.OrchestraID = c.OrchestraID and s.IsCurrent = 1)),
    c.InstrumentIDs,
    c.RoleIDs,
    c.IsAdmin,
    c.HasPlayingMembership,
    c.MembershipUniqueID,
    c.EffectiveCanEditOthers,
    c.RehearsalVisible
from @Context c;

/* RESULT SET 3 */
select
    ID            = i.ID,
    Name          = i.Name,
    [Order]       = i.[Order],
    RegisterName  = ir.Name,
    RegisterOrder = ir.[Order],
    GroupName     = ig.Name,
    GroupOrder    = ig.[Order],
    ig.OrchestraID
from Instrument i
join InstrumentRegister ir
  on ir.ID = i.InstrumentRegisterID
join InstrumentGroup ig
  on ig.ID = ir.InstrumentGroupID
where i.IsActive = 1
  and ir.IsActive = 1
  and ig.IsActive = 1;

/* RESULT SET 4 */
select
    r.ID,
    r.Name,
    r.OrchestraID,
    r.Parameters
from PersonRole r
where r.IsActive = 1;

/* RESULT SET 5 */
select
    o.ID,
    o.Name,
    o.UniqueID,
    o.Color1
from Orchestra o
where o.IsActive = 1;

/* RESULT SET 5 */
select	ao.ID,
		ao.AltText,
		ao.Value,
		ao.OrchestraID,
		ao.ColorLight,
		ao.ColorDark,
		ao.SymbolName,
		ao.Comment,
		ao.IsMandatory,
		ao.[Order]
from	AttendenceOption ao
join	@Universe u
	on	u.OrchestraID = ao.OrchestraID
where	IsActive = 1
";

            var ds = LoadDataSet(sql, ["p", "c", "i", "r", "o", "ao"], new Dictionary<string, object> { { "PersonUniqueID", personUniqueIDs } });
            var ps = ds.Tables["p"].Rows.Cast<DataRow>().Select(r => DataRowExtension.ToDataCollection(r)).ToArray();
            if (ps.Length == 0)
            {
                return Array.Empty<IPerson>();
            }

            var attendenceOptions = ds.Tables["ao"].Rows.Cast<DataRow>().Select(r => InstantiateAttendenceOption(DataRowExtension.ToDataCollection(r))).ToArray();
            var orchestras = ds.Tables["o"].Rows.Cast<DataRow>()
                .Select(r => DataRowExtension.ToDataCollection(r))
                .Select(r => InstantiateOrchestra(r, attendenceOptions.Where(ao => ao.OrchestraID == r.Get<int>("ID"))))
                .ToArray();
            var instruments = ds.Tables["i"].Rows.Cast<DataRow>().Select(r => InstantiateInstrument(DataRowExtension.ToDataCollection(r))).ToArray();
            var roles = ds.Tables["r"].Rows.Cast<DataRow>().Select(r => InstantiateRole(DataRowExtension.ToDataCollection(r))).ToArray();

            var contexts = ds.Tables["c"].Rows.Cast<DataRow>()
                .Select(r => r.ToDataCollection())
                .Select(r =>
                    {
                        var orchestraID = r.Get<int>("OrchestraID");
                        Orchestra orchestra = orchestras.Single(o => o.ID == orchestraID);
                        return new OrchestraContext(
                            r.Get<Guid?>("MembershipUniqueID"),
                            LoadPerson(r.Get<int>("PersonID")),
                            LoadSeason(r.Get<int?>("SeasonID"), orchestra),
                            r.Get<string>("InstrumentIDs")?.Split('|').Select(s => instruments.SingleOrDefault(i => i.ID.ToString() == s)).NotTheNulls().ToArray() ?? [],
                            r.Get<string>("RoleIDs")?.Split('|').Select(s => roles.Single(ro => ro.ID == int.Parse(s))).ToArray() ?? [],
                            r.Get<bool>("IsAdmin"),
                            r.Get<bool>("HasPlayingMembership"),
                            r.Get<CanEditOthers>("EffectiveCanEditOthers"),
                            r.Get<bool>("RehearsalVisible"),
                            orchestra);
                    })
                .ToArray();

            return ps
                .Select(p =>
                    {
                        var personContexts = contexts.Where(c => c.Person.ID == p.Get<int>("ID")).ToArray();
                        return new Person(
                            p.Get<int>("ID"),
                            p.Get<Guid>("UniqueID"),
                            p.Get<string>("FirstName"),
                            p.Get<string>("LastName"),
                            personContexts);
                    })
                .ToArray();
        }

        private IPerson LoadPerson(int id)
        {
            const string sql = @"
select	ID,
		UniqueID,
		FirstName,
		LastName
from	Person
where	IsActive = 1
    and ID = @ID
";

            return Load(sql, d => InstantiatePerson(d), new Dictionary<string, object> { { "ID", id } }).SingleOrDefault();
        }

        private Season LoadSeason(int? id, IOrchestra orchestra)
        {
            if (id == null)
            {
                return null;
            }

            string sql = @"
select  s.ID,
		s.OrchestraID,
		Caption,
		StartDate,
		EndDate,
		Comment
from	Season s
where	IsActive = 1
	and s.ID = @ID
";
            if (orchestra != null)
            {
                sql += @"
	and s.OrchestraID = @OrchestraID
";
            }

            return Load(sql, d => InstantiateSeason(d, orchestra), new Dictionary<string, object> { { "ID", id.Value }, { "OrchestraID", orchestra?.ID } }).SingleOrDefault();
        }

        public IEnumerable<IDate> GetOrchestraDates(Guid membershipID)
        {
            const string sql = @"
        select	r.ID,
        		r.EventTypeID,
        		r.DateAt,
        		r.LocationAt
        from	Person_Orchestra po
        join	Season s
        	on	s.OrchestraID = po.OrchestraID
        join	Event r
        	on	r.SeasonID = s.ID
        join	Person_PersonRole ppr
        	on	ppr.PersonID = po.PersonID
        join	PersonRole pr
        	on	pr.ID = ppr.PersonRoleID
        	and	pr.OrchestraID = po.OrchestraID
        join	EventType et
        	on	et.ID = r.EventTypeID
        where	PersonalizedGUID = @PGuid
        	and	po.IsActive = 1
        	and	s.IsCurrent = 1
        	and	s.IsActive = 1
        	and	r.IsActive = 1
        	and	ppr.IsActive = 1
        	and	pr.IsActive = 1
        	and	et.IsActive = 1
        ";

            var dates = Load(sql, InstatiateDate, new Dictionary<string, object> { { "PGuid", membershipID } }).ToArray();
            return dates;
        }

        public IEnumerable<IOrchestraDate> GetOrchestraDates(IEnumerable<Guid> membershipIDs)
        {
            return membershipIDs
                            .SelectMany(id =>
                            {
                                var dates = GetOrchestraDates(id);
                                var personInstrument = GetPersonInstrument(id);

                                return dates.Select(date => new OrchestraDate(personInstrument.Orchestra, date)).ToArray();
                            })
                            .ToArray();
        }

        public IEnumerable<IPersonEvent> GetAttendences(Guid membershipID)
        {
            const string sql = @"
select  p.ID as PersonID,
		p.FirstName,
		p.LastName,
		r.ID as EventID,
		r.DateAt,
		coalesce(pr.IsPresent, 0) as IsPresent
from	Season s
join	Person_Season_Instrument psi
	on	psi.SeasonID = s.ID
join	Person p
	on	p.ID = psi.PersonID
join	Event r
	on	r.SeasonID = s.ID
left join	Person_Event pr
	on	pr.PersonID = p.ID
	and	pr.EventID = r.ID
where	s.IsCurrent = 1
	and	s.OrchestraID = (
	    select	OrchestraID
	    from	Person_Orchestra
	    where	PersonalizedGUID = @PGuid)
	and s.IsActive = 1
	and psi.IsActive = 1
	and p.IsActive = 1
	and r.IsActive = 1
	and pr.IsActive = 1
";

            return Load(sql, InstantiateAttendence, new Dictionary<string, object> { { "PGuid", membershipID } })
                .ToArray();
        }

        public IEnumerable<IPersonEvent> GetAttendences(IEnumerable<Guid> membershipIDs)
        {
            return membershipIDs
                            .SelectMany(GetAttendences)
                            .ToArray();
        }

        public IPersonInstrument GetPersonInstrument(Guid membershipID)
        {
            const string sql = @"
declare @t table (name varchar(20), value int)
insert  @t
values ('all', 1),
        ('register', 2),
        ('group', 3),
        ('no', 4)

select	top 1
        PersonID						=	p.ID,
        PersonFirstName					=	p.FirstName,
        PersonLastName					=	p.LastName,
        PersonUniqueID					=	p.UniqueID,
        PersonCanEditOthers             =   isnull(json_value(r.Parameters, '$.canEditOthers'), 'no'),
        InstrumentID,
        InstrumentName					=	i.Name,
        InstrumentOrder					=	i.[Order],
        InstrumentRegisterName			=	ir.Name,
        InstrumentRegisterOrder			=	ir.[Order],
        InstrumentGroupName				=	ig.Name,
        InstrumentGroupOrder			=	ig.[Order],
        OrchestraID						=	o.ID,
        OrchestraName					=	o.Name,
        OrchestraUniqueID				=	o.UniqueID,
        OrchestraColor1                 =   o.Color1
from	Person_Orchestra po
join	Person p
    on	p.ID = po.PersonID
join	Person_Season_Instrument psi
    on	psi.PersonID = p.ID
join	Season s
    on	s.ID = psi.SeasonID
    and s.OrchestraID = po.OrchestraID
    and s.IsCurrent = 1
join	Instrument i
    on	i.ID = psi.InstrumentID
join	InstrumentRegister ir
    on	ir.ID = i.InstrumentRegisterID
join	InstrumentGroup ig
    on	ig.ID = ir.InstrumentGroupID
    and	ig.OrchestraID = po.OrchestraID
join	Person_PersonRole pr
    on	pr.PersonID = p.ID
join	PersonRole r
    on	r.ID = pr.PersonRoleID
join	@t t
    on	t.name = isnull(json_value(r.Parameters, '$.canEditOthers'), 'no')
join	Orchestra o
    on	o.ID = po.OrchestraID
where	po.PersonalizedGUID = @PGuid
    and r.OrchestraID = po.OrchestraID
    and po.IsActive = 1
    and p.IsActive = 1
    and psi.IsActive = 1
    and s.IsActive = 1
    and i.IsActive = 1
    and ir.IsActive = 1
    and ig.IsActive = 1
    and pr.IsActive = 1
    and r.IsActive = 1
    and o.IsActive = 1
order	by	t.value
";

            var attendenceOptions = LoadAttencenceOptions().ToArray();

            var raw = Load(
                sql,
                d => new Tuple<IPerson, IInstrument, IOrchestra>(
                    GetPersonInfo(d.Get<string>("PersonUniqueID")).SingleOrDefault(),
                    InstantiateInstrument(d, "Instrument"),
                    InstantiateOrchestra(d, attendenceOptions.Where(ao => ao.OrchestraID == d.Get<int>("OrchestraID")), "Orchestra")),
                new Dictionary<string, object> { { "PGuid", membershipID } })
                .ToArray();

            return raw
                 .GroupBy(t => t.Item1.ID)
                 .Select(g => new PersonInstrument(raw.First(r => r.Item1.ID == g.Key).Item1, g.Select(t => t.Item2), raw.First(r => r.Item1.ID == g.Key).Item3))
                 .SingleOrDefault();
        }

        public IEnumerable<IAttendenceOption> LoadAttencenceOptions()
        {
            const string sql = @"
select	ao.ID,
		ao.AltText,
		ao.Value,
		ao.OrchestraID,
		ao.ColorLight,
		ao.ColorDark,
		ao.SymbolName,
		ao.Comment,
		ao.IsMandatory,
		ao.[Order]
from	AttendenceOption ao
where	IsActive = 1
";
            return Load(sql, InstantiateAttendenceOption).ToArray();
        }

        public IEnumerable<IPersonInstrument> GetPersonInstruments(IEnumerable<Guid> membershipIDs)
        {
            return membershipIDs
                .Select(GetPersonInstrument)
                .ToArray();
        }

        public IEnumerable<IPersonInstrument> GetOtherPersonInstruments(Guid membershipID)
        {
            const string sql = @"
declare @seesOthers  varchar(20)
declare @personID    int
declare @orchestraID int
declare @registerID  int
declare @groupID     int

declare @t table (name varchar(20), value int)
insert  @t
values ('all', 1),
        ('register', 2),
        ('group', 3),
        ('no', 4)

select  top 1
        @seesOthers  =  json_value(pr.Parameters, '$.seesOthers'),
        @personID    =  po.PersonID,
        @orchestraID =  po.OrchestraID,
        @registerID  =  i.InstrumentRegisterID,
        @groupID     =  ir.InstrumentGroupID
from    Person_PersonRole ppr
join    Person_Orchestra po
    on  po.PersonID = ppr.PersonID
join    PersonRole pr
    on  pr.ID = ppr.PersonRoleID
    and pr.OrchestraID = po.OrchestraID
join    Person_Season_Instrument psi
    on  psi.PersonID = po.PersonID
join    Instrument i
    on  i.ID = psi.InstrumentID
join    InstrumentRegister ir
    on  ir.ID = i.InstrumentRegisterID
join    vSeason s
    on  s.ID = psi.SeasonID
    and s.OrchestraID = po.OrchestraID
join    @t t
    on  t.name = json_value(pr.Parameters, '$.seesOthers')
where   po.PersonalizedGUID = @PGuid
    and s.IsCurrent = 1
    and ppr.IsActive = 1
    and po.IsActive = 1
    and pr.IsActive = 1
    and psi.IsActive = 1
    and i.IsActive = 1
    and ir.IsActive = 1
    and s.IsActive = 1
order   by  t.value

--select '@seesOthers  = ' + convert(varchar(max), @seesOthers )
--select '@personID    = ' + convert(varchar(max), @personID   )
--select '@orchestraID = ' + convert(varchar(max), @orchestraID)
--select '@registerID  = ' + convert(varchar(max), @registerID )
--select '@groupID     = ' + convert(varchar(max), @groupID    )

select  PersonID                =   p.ID,
        PersonFirstName         =   p.FirstName,
        PersonLastName          =   p.LastName,
        PersonUniqueID          =   p.UniqueID,
        PersonCanEditOthers     =   isnull(json_value(r.Parameters, '$.canEditOthers'), 'no'),
        InstrumentID            =   i.ID,
        InstrumentName          =   i.Name,
        InstrumentOrder         =   i.[Order],
        InstrumentRegisterName  =   ir.Name,
        InstrumentRegisterOrder =   ir.[Order],
        InstrumentGroupName     =   ig.Name,
        InstrumentGroupOrder    =   ig.[Order],
        OrchestraID             =   o.ID,
        OrchestraName           =   o.Name,
        OrchestraUniqueID       =   o.UniqueID,
        OrchestraColor1         =   o.Color1
from    Person_Orchestra po
join    Person p
    on  p.ID = po.PersonID
join    Person_Season_Instrument psi
    on  psi.PersonID = po.PersonID
join    vSeason s
    on  s.ID = psi.SeasonID
    and s.OrchestraID = po.OrchestraID
join    Instrument i
    on  i.ID = psi.InstrumentID
join    InstrumentRegister ir
    on  ir.ID = i.InstrumentRegisterID
join    InstrumentGroup ig
    on  ig.ID = ir.InstrumentGroupID
    and ig.OrchestraID = po.OrchestraID
join    Person_PersonRole pr
    on  pr.PersonID = p.ID
join    PersonRole r
    on  r.ID = pr.PersonRoleID
    and r.OrchestraID = po.OrchestraID
join    Orchestra o
    on  o.ID = po.OrchestraID
where   po.OrchestraID = @orchestraID
    and s.IsCurrent = 1
    and p.ID <> @personID
    and (
            @seesOthers = 'all'
        or  (@seesOthers = 'register' and i.InstrumentRegisterID = @registerID)
        or  (@seesOthers = 'group' and ig.ID = @groupID)
    )
    and po.IsActive = 1
    and p.IsActive = 1
    and psi.IsActive = 1
    and s.IsActive = 1
    and i.IsActive = 1
    and ir.IsActive = 1
    and ig.IsActive = 1
    and pr.IsActive = 1
    and r.IsActive = 1
    and o.IsActive = 1
        ";

            var attendenceOptions = LoadAttencenceOptions().ToArray();
            var raw = Load(
                sql,
                d =>
                {
                    IPerson person = GetPersonInfo(d.Get<string>("PersonUniqueID")).Single(p => p.ID == d.Get<int>("PersonID"));
                    return new Tuple<IPerson, IInstrument, IOrchestra>(
                                        person,
                                        InstantiateInstrument(d, "Instrument"),
                                        InstantiateOrchestra(d, attendenceOptions.Where(ao => ao.OrchestraID == d.Get<int>("OrchestraID")), "Orchestra"));
                },
                new Dictionary<string, object> { { "PGuid", membershipID } })
                .ToArray();

            return raw
                 .GroupBy(t => t.Item1.ID)
                 .Select(g => new PersonInstrument(raw.First(r => r.Item1.ID == g.Key).Item1, g.Select(t => t.Item2), raw.First(r => r.Item1.ID == g.Key).Item3))
                 .ToArray();
        }

        public IEnumerable<IPersonInstrument> GetOtherPersonInstruments(IEnumerable<Guid> membershipIDs)
        {
            return membershipIDs.SelectMany(GetOtherPersonInstruments)
                            .ToArray();
        }

        public void UpdateAttendence(int personID, int dateID, IsPresent? isPresent)
        {
            const string sql = @"--
declare @id int = null;

select	@id = ID
from	Person_Event
where	PersonID = @PersonID
	and	EventID = @EventID
	and IsActive = 1

if	@id is null begin
	if	@IsPresent is not null begin
		insert	Person_Event (PersonID, EventID, IsPresent)
		select	@PersonID, @EventID, @IsPresent
	end
end
else begin
	if	@IsPresent is null begin
		delete	Person_Event
		where	PersonID = @PersonID
			and	EventID = @EventID
	end
	else begin
		update	Person_Event        
		set		IsPresent = @IsPresent
		where	PersonID = @PersonID
			and	EventID = @EventID
	end
end

select	@id
";

            Execute(sql, new Dictionary<string, object>
            {
                { "PersonID", personID },
                { "EventID", dateID },
                { "IsPresent", (int?)isPresent }
            });
        }

        public IEditableOrchestra GetEditableOrchestra(Guid orchestraGuid)
        {
            const string sql = @"
select  o.ID, 
        o.UniqueID, 
        o.Name, 
        o.Comment as Description, 
        '' as Location, 
        o.Color1 
from    Orchestra o where   o.UniqueID = @OrchestraId and o.IsActive = 1

select  ao.ID,
        ao.AltText,
        ao.Value,
        ao.OrchestraID,
        ao.ColorLight,
        ao.ColorDark,
        ao.SymbolName,
        ao.Comment,
        ao.IsMandatory,
        ao.[Order]
from    AttendenceOption ao
join    Orchestra o
    on  o.ID = ao.OrchestraID
where   o.UniqueID = @OrchestraId
    and ao.IsActive = 1
order by ao.[Order]

select  p.ID,
        p.UniqueID,
        p.FirstName,
        p.LastName,
        Roles = (
            select  string_agg(convert(varchar(max), pr.ID), '|')
            from    Person_PersonRole ppr2
            join    PersonRole pr
                on  pr.ID = ppr2.PersonRoleID
            where   ppr2.PersonID = p.ID
                and ppr2.IsActive = 1
                and pr.IsActive = 1
                and pr.OrchestraID = (
                    select top 1 ID
                    from   Orchestra
                    where  UniqueID = @OrchestraId
                        and IsActive = 1
                )
        )
        ,Instruments = (
            select string_agg(convert(varchar(max), i.ID), '|')
            from Person_Season_Instrument psi
            join Season s2 on s2.ID = psi.SeasonID
            join Instrument i on i.ID = psi.InstrumentID
            where psi.PersonID = p.ID
              and psi.IsActive = 1
              and i.IsActive = 1
              and s2.OrchestraID = (
                    select top 1 ID
                    from   Orchestra
                    where  UniqueID = @OrchestraId
                        and IsActive = 1
                )
        )
from    Person p
where   p.IsActive = 1
    and exists (
        select  1
        from    Person_PersonRole ppr
        join    PersonRole pr
            on  pr.ID = ppr.PersonRoleID
        where   ppr.PersonID = p.ID
            and ppr.IsActive = 1
            and pr.IsActive = 1
            and pr.OrchestraID = (
                select top 1 ID
                from   Orchestra
                where  UniqueID = @OrchestraId
                    and IsActive = 1
            )
    )
order by p.LastName, p.FirstName, p.ID
";
            var ds = LoadDataSet(sql, ["o", "ao", "p"], new Dictionary<string, object> { { "OrchestraId", orchestraGuid } });

            var orchestraRow = ds.Tables["o"].Rows.Cast<DataRow>().FirstOrDefault();
            if (orchestraRow == null)
            {
                return null;
            }

            var o = DataRowExtension.ToDataCollection(orchestraRow);

            var attendenceOptions = ds.Tables["ao"].Rows.Cast<DataRow>()
                .Select(r => InstantiateAttendenceOption(DataRowExtension.ToDataCollection(r)))
                .ToArray();

            var colors = new OrchestraColors(o.Get<string>("Color1"));

            // Load seasons for this orchestra (Caption, StartDate, Comment, IsActive). EndDate is computed elsewhere.
            const string seasonsSql = @"
select  s.ID,
        s.OrchestraID,
        s.Caption,
        s.StartDate,
        s.EndDate,
        s.Comment,
        s.IsActive
from    vSeason s
where   s.OrchestraID = (select top 1 ID from Orchestra where UniqueID = @OrchestraId and IsActive = 1)
order by s.StartDate desc
";

            var orchestraModel = new Orchestra(o.Get<int>("ID"), o.Get<Guid>("UniqueID"), o.Get<string>("Name"), o.Get<string>("Color1"), attendenceOptions);

            var seasons = Load(seasonsSql,
                d => new Season(
                    d.Get<int>("ID"),
                    d.Get<string>("Caption"),
                    d.Get<DateTime?>("StartDate"),
                    d.Get<DateTime?>("EndDate"),
                    orchestraModel),
                new Dictionary<string, object> { { "OrchestraId", orchestraGuid } })
                .ToArray();

            var persons = ds.Tables["p"].Rows.Cast<DataRow>()
                .Select(r => DataRowExtension.ToDataCollection(r))
                .Select(d =>
                {
                    var rolesString = d.Get<string>("Roles");
                    var roles = string.IsNullOrEmpty(rolesString)
                        ? Array.Empty<int>()
                        : rolesString.Split('|').Where(s => !string.IsNullOrEmpty(s)).Select(int.Parse).ToArray();

                    var instString = d.Get<string>("Instruments");
                    var insts = string.IsNullOrEmpty(instString)
                        ? Array.Empty<int>()
                        : instString.Split('|').Where(s => !string.IsNullOrEmpty(s)).Select(int.Parse).ToArray();

                    return (IEditablePerson)new EditablePerson(d.Get<string>("FirstName"), d.Get<string>("LastName"), roles, insts);
                })
                .ToArray();

            var roles = GetRoles(o.Get<int>("ID")).ToArray();

            // load instruments for this orchestra
            const string instrumentsSql = @"
select  i.ID            as InstrumentID,
        i.Name          as InstrumentName,
        i.[Order]       as InstrumentOrder,
        ir.Name         as InstrumentRegisterName,
        ir.[Order]      as InstrumentRegisterOrder,
        ig.Name         as InstrumentGroupName,
        ig.[Order]      as InstrumentGroupOrder
from    Instrument i
join    InstrumentRegister ir
    on  ir.ID = i.InstrumentRegisterID
join    InstrumentGroup ig
    on  ig.ID = ir.InstrumentGroupID
where   ig.OrchestraID = (select top 1 ID from Orchestra where UniqueID = @OrchestraId and IsActive = 1)
    and i.IsActive = 1
order by ig.[Order], ir.[Order], i.[Order]
";

            var instruments = Load(instrumentsSql, d => InstantiateInstrument(d, "Instrument"), new Dictionary<string, object> { { "OrchestraId", orchestraGuid } }).ToArray();

            return new EditableOrchestra(
                o.Get<Guid>("UniqueID"),
                o.Get<string>("Name"),
                o.Get<string>("Description"),
                o.Get<string>("Location"),
                persons,
                roles,
                instruments,
                colors,
                attendenceOptions,
                seasons);
        }

        public IOrchestraColors GetOrchestraColors(int orchestraID)
        {
            const string sql = @"
select  o.Color1
from    Orchestra o
where   o.ID = @ID;
";

            return new OrchestraColors(LoadScalar<string>(sql, new Dictionary<string, object> { { "ID", orchestraID } }));
        }

        public IEnumerable<IPersonRole> GetRoles(int orchestraID)
        {
            const string sql = @"
select  r.ID,
        r.Name,
        r.Parameters
from    PersonRole r
where   r.IsActive = 1
    and r.OrchestraID = @OrchestraID
order by r.Name
";

            return Load(sql, InstantiatePersonRole, new Dictionary<string, object> { { "OrchestraID", orchestraID } });
        }

        public IEnumerable<IEditablePerson> GetParticipants(Guid orchestraGuid, int seasonID)
        {
            const string sql = @"
declare @OrchestraID int = (select top 1 ID from Orchestra where UniqueID = @OrchestraGuid and IsActive = 1);

select  p.ID,
        p.UniqueID,
        p.FirstName,
        p.LastName,
        Roles = (
            select string_agg(convert(varchar(max), pr.ID), '|')
            from Person_PersonRole ppr2
            join PersonRole pr on pr.ID = ppr2.PersonRoleID
            where ppr2.PersonID = p.ID
              and ppr2.IsActive = 1
              and pr.IsActive = 1
              and pr.OrchestraID = @OrchestraID
        ),
        Instruments = (
            select string_agg(convert(varchar(max), i.ID), '|')
            from Person_Season_Instrument psi
            join Instrument i on i.ID = psi.InstrumentID
            where psi.PersonID = p.ID
              and psi.SeasonID = @SeasonID
              and psi.IsActive = 1
              and i.IsActive = 1
        )
from Person p
join Person_Orchestra po on po.PersonID = p.ID and po.OrchestraID = @OrchestraID and po.IsActive = 1
where p.IsActive = 1
  and (
        exists(select 1 from Person_Season_Instrument psi where psi.PersonID = p.ID and psi.SeasonID = @SeasonID and psi.IsActive = 1)
        or exists(select 1 from Person_PersonRole ppr3 join PersonRole r on r.ID = ppr3.PersonRoleID where ppr3.PersonID = p.ID and ppr3.IsActive = 1 and r.IsActive = 1 and r.OrchestraID = @OrchestraID)
      )
order by p.LastName, p.FirstName
";

            return Load(sql,
                d =>
                {
                    var rolesString = d.Get<string>("Roles");
                    var roles = string.IsNullOrEmpty(rolesString)
                        ? Array.Empty<int>()
                        : rolesString.Split('|').Where(s => !string.IsNullOrEmpty(s)).Select(int.Parse).ToArray();

                    var instString = d.Get<string>("Instruments");
                    var insts = string.IsNullOrEmpty(instString)
                        ? Array.Empty<int>()
                        : instString.Split('|').Where(s => !string.IsNullOrEmpty(s)).Select(int.Parse).ToArray();

                    return (IEditablePerson)new EditablePerson(d.Get<string>("FirstName"), d.Get<string>("LastName"), roles, insts);
                },
                new Dictionary<string, object> { { "OrchestraGuid", orchestraGuid }, { "SeasonID", seasonID } }).ToArray();
        }

        private EventType LoadEventType(int id)
        {
            const string sql = @"
select	ID,
        Name
from	EventType
where	ID = @ID
";
            return Load(sql, InstantiateEventType, new Dictionary<string, object> { { "ID", id } }).SingleOrDefault();
        }

        private static IPersonRole InstantiatePersonRole(IDataCollection d)
        {
            return new PersonRole(
                d.Get<int>("ID"),
                d.Get<string>("Name"),
                d.Get<string>("Parameters")
            );
        }

        private static Person InstantiatePerson(IDataCollection d, string identifiersPrefix = "", IEnumerable<IMembership> memberships = null)
        {
            return new Person(
                d.Get<int>($"{identifiersPrefix}ID"),
                d.Get<Guid>($"{identifiersPrefix}UniqueID"),
                d.Get<string>($"{identifiersPrefix}FirstName"),
                d.Get<string>($"{identifiersPrefix}LastName"),
                memberships);
        }

        private IDate InstatiateDate(IDataCollection d)
        {
            var eventTypeID = d.Get<int>("EventTypeID");
            return new Event(
                d.Get<int>("ID"),
                d.Get<DateTime>("DateAt"),
                d.Get<string>("LocationAt"),
                LoadEventType(eventTypeID));
        }

        private static EventType InstantiateEventType(IDataCollection d)
        {
            return new EventType(
                d.Get<int>("ID"),
                d.Get<string>("Name"));
        }

        private static PersonEvent InstantiateAttendence(IDataCollection d)
        {
            return new PersonEvent(
                d.Get<int>("PersonID"),
                d.Get<int>("EventID"),
                d.Get<IsPresent>("IsPresent"));
        }

        private static Instrument InstantiateInstrument(IDataCollection d, string identifiersPrefix = "")
        {
            return new Instrument(
                d.Get<int>($"{identifiersPrefix}ID"),
                d.Get<string>($"{identifiersPrefix}Name"),
                d.Get<int>($"{identifiersPrefix}Order"),
                d.Get<string>($"{identifiersPrefix}RegisterName"),
                d.Get<int>($"{identifiersPrefix}RegisterOrder"),
                d.Get<string>($"{identifiersPrefix}GroupName"),
                d.Get<int>($"{identifiersPrefix}GroupOrder"));
        }

        private static Orchestra InstantiateOrchestra(IDataCollection d, IEnumerable<IAttendenceOption> attendenceOptions, string identifiersPrefix = "")
        {
            return new Orchestra(
                d.Get<int>($"{identifiersPrefix}ID"),
                d.Get<Guid>($"{identifiersPrefix}UniqueID"),
                d.Get<string>($"{identifiersPrefix}Name"),
                d.Get<string>($"{identifiersPrefix}Color1"),
                attendenceOptions);
        }

        private static Role InstantiateRole(IDataCollection d)
        {
            return new Role(
                d.Get<int>("ID"),
                d.Get<string>("Name"),
                d.Get<string>("Parameters"));
        }

        private static Season InstantiateSeason(IDataCollection d, IOrchestra orchestra)
        {
            return new Season(
                d.Get<int>("ID"),
                d.Get<string>("Caption"),
                d.Get<DateTime>("StartDate"),
                d.Get<DateTime>("EndDate"),
                orchestra);
        }

        private static IAttendenceOption InstantiateAttendenceOption(IDataCollection d)
        {
            var valueString = d.Get<string>("Value");
            IsPresent? value = valueString.IsNullOrEmpty() ? null : d.Get<IsPresent>("Value");
            return new AttendenceOption(
                d.Get<int>("ID"),
                d.Get<string>("AltText"),
                value,
                d.Get<int>("OrchestraID"),
                d.Get<string>("ColorLight"),
                d.Get<string>("ColorDark"),
                d.Get<string>("SymbolName"),
                d.Get<string>("Comment"),
                d.Get<bool>("IsMandatory"),
                d.Get<int>("Order"),
                value != null);
        }

        public void SaveEditableOrchestra(Guid orchestraGuid, IEnumerable<Wheeeee.Core.Interfaces.Collections.IDataCollection> seasons)
        {
            if (seasons == null) return;

            const string getOrc = @"select top 1 ID from Orchestra where UniqueID = @UniqueID and IsActive = 1";
            var orchestraID = LoadScalar<int?>(getOrc, new Dictionary<string, object> { { "UniqueID", orchestraGuid } });
            if (orchestraID == null) return;

            foreach (var dc in seasons)
            {
                if (dc == null) continue;

                int id = 0;
                try { id = dc.Get<int>("ID"); } catch { id = 0; }

                string caption = string.Empty;
                try { caption = dc.Get<string>("Caption"); } catch { caption = string.Empty; }

                string comment = string.Empty;
                try { comment = dc.Get<string>("Comment"); } catch { comment = string.Empty; }

                bool isActive = false;
                try { isActive = dc.Get<bool>("IsActive"); } catch { isActive = false; }

                DateTime? startDate = null;
                try { startDate = dc.Get<DateTime?>("StartDate"); } catch { startDate = null; }

                if (id == 0)
                {
                    const string insertSql = @"
insert into Season(OrchestraID, Caption, StartDate, IsActive, Comment)
values(@OrchestraID, @Caption, @StartDate, @IsActive, @Comment)
";
                    Execute(insertSql, new Dictionary<string, object>
                    {
                        { "OrchestraID", orchestraID.Value },
                        { "Caption", caption },
                        { "StartDate", (object)startDate ?? DBNull.Value },
                        { "IsActive", isActive ? 1 : 0 },
                        { "Comment", comment }
                    });
                }
                else
                {
                    const string updateSql = @"
update Season
set Caption = @Caption,
    StartDate = @StartDate,
    IsActive = @IsActive,
    Comment = @Comment
where ID = @ID and OrchestraID = @OrchestraID
";
                    Execute(updateSql, new Dictionary<string, object>
                    {
                        { "ID", id },
                        { "OrchestraID", orchestraID.Value },
                        { "Caption", caption },
                        { "StartDate", (object)startDate ?? DBNull.Value },
                        { "IsActive", isActive ? 1 : 0 },
                        { "Comment", comment }
                    });
                }
            }
        }
    }
}