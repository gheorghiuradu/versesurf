using System.Collections.Generic;

namespace LicensingService.Models
{
    public partial class AscapResponse
    {
        public List<Result> Result { get; set; }
        public object Error { get; set; }
        public Meta Meta { get; set; }
    }

    public partial class Meta
    {
        public long Limit { get; set; }
        public string DetailedSharesEnabled { get; set; }
        public object Previous { get; set; }
        public long TotalCount { get; set; }
        public long Page { get; set; }
        public object Next { get; set; }
        public long AttCount { get; set; }
    }

    public partial class Result
    {
        public List<InterestedParty> InterestedParties { get; set; }
        public string WorkTitle { get; set; }
        public string AlternateTitles { get; set; }
        public double TotalPublisherAscapShare { get; set; }
        public string WorkTitleTypeCde { get; set; }
        public object LieFlag { get; set; }
        public List<Performer> Performers { get; set; }
        public string HoldIndicator { get; set; }
        public string IswcCde { get; set; }
        public string TotalAscapShareMessage { get; set; }
        public string DisputeFlag { get; set; }
        public string SpecialPaymentIndicator { get; set; }
        public long PerformerCount { get; set; }
        public long WorkId { get; set; }
        public double TotalWriterAscapShare { get; set; }
        public double TotalAscapShare { get; set; }
        public string ResourceUri { get; set; }
        public long WriterCount { get; set; }
    }

    public partial class InterestedParty
    {
        public long PartyId { get; set; }
        public object LieMandateStartDate { get; set; }
        public string PayIndicator { get; set; }
        public string SocietyName { get; set; }
        public long IpiNaNum { get; set; }
        public string FullName { get; set; }
        public string ResourceUri { get; set; }
        public string RoleCde { get; set; }
        public List<Phone> Phone { get; set; }
        public List<Email> Email { get; set; }
        public List<Address> Address { get; set; }
    }

    public partial class Address
    {
        public string CountryCde { get; set; }
        public string StateDesc { get; set; }
        public string StateCde { get; set; }
        public string Name3 { get; set; }
        public string Province { get; set; }
        public string CountryDesc { get; set; }
        public string PostalCde { get; set; }
        public string Line1 { get; set; }
        public string Name2 { get; set; }
        public string City { get; set; }
        public string Line2 { get; set; }
    }

    public partial class Email
    {
        public string EmailEmail { get; set; }
    }

    public partial class Phone
    {
        public string PhoneNumber { get; set; }
    }

    public partial class Performer
    {
        public string FullName { get; set; }
        public string ResourceUri { get; set; }
    }

    //public enum DetailedSharesEnabled { Empty, N, Y };

    //public enum RoleCde { P, W };
}