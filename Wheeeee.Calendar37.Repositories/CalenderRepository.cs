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

        public IEnumerable<IMembership> GetMembershipsByUniqueIDs(IEnumerable<Guid> membershipIDs)
        {
            string sql = $@"
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
where	po.PersonalizedGUID in ({membershipIDs.Select(id => id.ToString("D").SurroundWith("'")).Join(",")})
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
";

            DataSet dataSet = LoadDataSet(sql, ["p", "m", "i"]);
            DataRow row = dataSet.Tables["p"].Rows.Cast<DataRow>().FirstOrDefault();
            if (row == null)
            {
                return Enumerable.Empty<IMembership>();
            }
            var personData = DataRowExtension.ToDataCollection(row);
            personData.Add("CanEditOthers", dataSet.Tables["p"].Rows.Cast<DataRow>().Select(r => r["CanEditOthers"].To<CanEditOthers>()).Min());
            var person = InstantiatePerson(personData);
            var allInstruments = dataSet.Tables["i"]
                .Rows.Cast<DataRow>()
                .Select(r => InstantiateInstrument(DataRowExtension.ToDataCollection(r)))
                .ToArray();
            var memberships = dataSet.Tables["m"]
                .Rows.Cast<DataRow>()
                .Select(r => r.ToDataCollection())
                .Select(r =>
                {
                    var orchestra = InstantiateOrchestra(r, "Orchestra");

                    var season = new Season(
                        r.Get<int>("SeasonID"),
                        r.Get<string>("SeasonCaption"),
                        orchestra);

                    var rolesDataRows = r.Get<string>("OrchestraRoles")?.Split('|') ?? Array.Empty<string>();
                    var roles = rolesDataRows
                        .Select(s =>
                        {
                            var parts = s.Split(';');
                            return new Role(int.Parse(parts[0]), parts[1]);
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
where	po.PersonalizedGUID = '{membershipID:D}'
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
	and po.PersonalizedGUID = '{membershipID:D}'
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
";

            DataSet dataSet = LoadDataSet(sql, ["p", "m", "i"]);
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
                persons.Min(p => p.CanEditOthers));
            var allInstruments = dataSet.Tables["i"]
                 .Rows.Cast<DataRow>()
                 .Select(r => InstantiateInstrument(DataRowExtension.ToDataCollection(r)))
                 .ToArray();
            var membership = dataSet.Tables["m"]
                .Rows.Cast<DataRow>()
                .Select(r => r.ToDataCollection())
                .Select(r =>
                {
                    var orchestra = InstantiateOrchestra(r, "Orchestra");

                    var season = new Season(
                        r.Get<int>("SeasonID"),
                        r.Get<string>("SeasonCaption"),
                        orchestra);

                    var rolesDataRows = r.Get<string>("OrchestraRoles")?.Split('|') ?? Array.Empty<string>();
                    var roles = rolesDataRows
                        .Select(s =>
                        {
                            var parts = s.Split(';');
                            return new Role(int.Parse(parts[0]), parts[1]);
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
select	PersonID						=	p.ID,
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
		OrchestraUniqueID				=	o.UniqueID
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
";

            var raw = Load(
                sql,
                d => new Tuple<IPerson, IInstrument, IOrchestra>(InstantiatePerson(d, "Person"), InstantiateInstrument(d, "Instrument"), InstantiateOrchestra(d, "Orchestra")),
                new Dictionary<string, object> { { "PGuid", membershipID } })
                .ToArray();

            return raw
                 .GroupBy(t => t.Item1.ID)
                 .Select(g => new PersonInstrument(raw.First(r => r.Item1.ID == g.Key).Item1, g.Select(t => t.Item2), raw.First(r => r.Item1.ID == g.Key).Item3))
                 .SingleOrDefault();
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

select  @seesOthers  = JSON_VALUE(pr.Parameters, '$.seesOthers'),
        @personID    = po.PersonID,
        @orchestraID = po.OrchestraID,
        @registerID  = i.InstrumentRegisterID,
        @groupID     = ir.InstrumentGroupID
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
join    Season s
    on  s.ID = psi.SeasonID
    and s.OrchestraID = po.OrchestraID
where   po.PersonalizedGUID = @PGuid
    and s.IsCurrent = 1
    and ppr.IsActive = 1
    and po.IsActive = 1
    and pr.IsActive = 1
    and psi.IsActive = 1
    and i.IsActive = 1
    and ir.IsActive = 1
    and s.IsActive = 1

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
        OrchestraUniqueID       =   o.UniqueID
from    Person_Orchestra po
join    Person p
    on  p.ID = po.PersonID
join    Person_Season_Instrument psi
    on  psi.PersonID = po.PersonID
join    Season s
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

            var raw = Load(
                sql,
                d => new Tuple<IPerson, IInstrument, IOrchestra>(InstantiatePerson(d, "Person"), InstantiateInstrument(d, "Instrument"), InstantiateOrchestra(d, "Orchestra")),
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

        public void UpdateAttendence(int personID, int dateID, IsPresent isPresent)
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
                { "IsPresent", (int)isPresent }
            });
        }

        public IEditableOrchestra GetEditableOrchestra(Guid orchestraGuid)
        {
            throw new NotImplementedException();
        }

        private IEventType LoadEventType(int id)
        {
            const string sql = @"
select	ID,
        Name
from	EventType
where	ID = @ID
";
            return Load(sql, InstantiateEventType, new Dictionary<string, object> { { "ID", id } }).SingleOrDefault();
        }

        private static Person InstantiatePerson(IDataCollection d, string identifiersPrefix = "")
        {
            return new Person(
                d.Get<int>($"{identifiersPrefix}ID"),
                d.Get<Guid>($"{identifiersPrefix}UniqueID"),
                d.Get<string>($"{identifiersPrefix}FirstName"),
                d.Get<string>($"{identifiersPrefix}LastName"),
                d.Get<CanEditOthers>($"{identifiersPrefix}CanEditOthers"));
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

        private static IEventType InstantiateEventType(IDataCollection d)
        {
            return new EventType(
                d.Get<int>("ID"),
                d.Get<string>("Name"));
        }

        private static IPersonEvent InstantiateAttendence(IDataCollection d)
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

        private static IOrchestra InstantiateOrchestra(IDataCollection d, string prefix)
        {
            return new Orchestra(
                d.Get<int>($"{prefix}ID"),
                d.Get<Guid>($"{prefix}UniqueID"),
                d.Get<string>($"{prefix}Name"));
        }
    }
}