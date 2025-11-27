using FluentValidation;
using Movies.Application.Models;
using System;
using System.Linq;

namespace Movies.Application.Validators
{
    public class GetAllMoviesOptionsValidator : AbstractValidator<GetAllMoviesOptions>
    {
        private static readonly List<string> _accepatableSortByFields =
        new List<string>
        {
            "title", "yearofrelease"
        };

        public GetAllMoviesOptionsValidator()
        {
            RuleFor(x => x.YearOfRelease)
                .LessThanOrEqualTo(DateTime.Now.Year);

            RuleFor(x => x.SortField)
                .Must(x=> x is null || _accepatableSortByFields.Contains(x, StringComparer.OrdinalIgnoreCase))
                .WithMessage("You can only sort by 'title' or 'yearofrelease'.");
        }
    }
}
