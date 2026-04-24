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
        ) t
group   by  t.ID,
            t.UniqueID,
            t.FirstName,
            t.LastName