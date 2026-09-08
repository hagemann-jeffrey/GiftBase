using GiftBase.Core.Dtos;
using GiftBase.Core.Enums;

namespace GiftBase.Core.Entities;

public class Occasion
{
    private Occasion() { }

    public Occasion(OccasionType type, string? title, DateOnly date, bool isRecurring, int personId)
    {
        Type = type;
        Title = title;
        Date = date;
        IsRecurring = isRecurring;
        PersonId = personId;
    }

    public int Id { get; private set; }
    public OccasionType Type { get; private set; }
    public string? Title { get; private set; }
    public DateOnly Date { get; private set; }
    public bool IsRecurring { get; private set; }
    public int PersonId { get; private set; }

    public Person Person { get; private set; } = null!;
    public List<Gift> Gifts { get; private set; } = [];

    public void Update(OccasionUpdateDto occasionUpdateDto)
    {
        Title = occasionUpdateDto.Title;
        Date = occasionUpdateDto.Date;
        IsRecurring = occasionUpdateDto.IsRecurring;
    }

    public void SetDate(DateOnly date)
    {
        Date = date;
    }

    public DateOnly GetNextOccurrence(DateOnly today)
    {
        if (!IsRecurring)
        {
            return Date;
        }

        var occurrenceInCurrentYear = BuildOccurrence(today.Year);

        return occurrenceInCurrentYear < today ? BuildOccurrence(today.Year + 1) : occurrenceInCurrentYear;
    }

    public bool IsPast(DateOnly today) => !IsRecurring && Date < today;

    private DateOnly BuildOccurrence(int year)
    {
        var day = Math.Min(Date.Day, DateTime.DaysInMonth(year, Date.Month));

        return new DateOnly(year, Date.Month, day);
    }
}
