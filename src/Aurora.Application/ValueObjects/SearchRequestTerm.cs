namespace Aurora.Application.ValueObjects;

public class SearchRequestTerm : ValueObject
{
    private readonly IEnumerable<string> _terms;
    private SearchRequestTerm(IEnumerable<string> terms)
    {
        //Guarantee order for value comparison
        _terms = terms.OrderBy(x => x);
    }

    public IEnumerable<string> Terms => _terms;

    public static SearchRequestTerm CreateAnd(IEnumerable<string> terms) =>
        new(terms);

    public static SearchRequestTerm ParseString(string str) =>
        new(str.Split(','));

    public override string ToString() =>
        String.Join(',', _terms.Distinct());

    protected override IEnumerable<object> GetEqualityComponents()
    {
        foreach (var term in _terms)
        {
            yield return term;
        }
    }
}