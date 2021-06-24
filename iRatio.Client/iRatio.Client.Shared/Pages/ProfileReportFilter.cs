using System;
using System.Collections.Generic;
using System.Text;

namespace UnoTest.Client.Pages
{
    public class ProfileReportFilter
    {
        public string Name { get; set; }

        public Func<DTO.User.Report, bool> Filter { get; set; }

        public override string ToString() => Name;
    }
}
