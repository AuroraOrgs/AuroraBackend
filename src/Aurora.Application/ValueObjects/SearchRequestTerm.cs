namespace Aurora.Application.ValueObjects;

public class SearchOptionTerm : ValueObject
{
    private readonly IEnumerable<string> _terms;
    private SearchOptionTerm(IEnumerable<string> terms)
    {
        //Guarantee order for value comparison
        _terms = terms.OrderBy(x => x);
    }

    public IEnumerable<string> Terms => _terms;

    public static SearchOptionTerm CreateAnd(IEnumerable<string> terms) =>
        new(terms);

    public static SearchOptionTerm ParseString(string str) =>
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