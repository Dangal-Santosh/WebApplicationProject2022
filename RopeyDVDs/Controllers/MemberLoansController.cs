using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RopeyDVDs.Data;
using RopeyDVDs.Models;
using RopeyDVDs.Models.DTO;

namespace RopeyDVDs.Controllers
{
    public class MemberLoansController : Controller
    {
        private readonly ApplicationDbContext applicationDbContext;

        public MemberLoansController(ApplicationDbContext db)
        {
            applicationDbContext = db;
        }

        //Question No 8 (Showing all detils of the members who have taken loans)
        //Authentication
        [Authorize(Roles = "Manager, Assistant")]
        /// <summary>
        /// It gets all the members from the database, then it gets the membership category of each
        /// member, then it gets the total number of loans for each member, then it creates a new object
        /// of type MemberLoanDetailsDTO and adds the member's details to it, then it adds the object to
        /// a list of type MemberLoanDetailsDTO, then it orders the list by the member's first name,
        /// then it passes the list to the view
        /// </summary>
        /// <returns>
        /// A list of MemberLoanDetailsDTO objects.
        /// </returns>
        public IActionResult MemberLoanDetails()
        {
            List<MemberLoanDetailsDTO> memberLoanDetailsDtos = new List<MemberLoanDetailsDTO>();
            List<Member> memberList = applicationDbContext.Member.Include(x => x.MembershipCategory).ToList();
            if (memberList != null)
            {
                foreach (Member member in memberList)
                {
                    var membershipCategory = applicationDbContext.MembershipCategory.Where(x => x.MembershipCategoryNumber == member.MembershipCategory.MembershipCategoryNumber).First();
                    int totalLoan = applicationDbContext.Loan.Include(x => x.Member).Where(x => x.Member == member
                           && x.Status == "loaned").ToArray().Length;

                    if (totalLoan > 0)
                    {
                        MemberLoanDetailsDTO memberLoanDetail = new MemberLoanDetailsDTO();
                        memberLoanDetail.Address = member.MemberAddress;
                        memberLoanDetail.FirstName = member.MemberFirstName;
                        memberLoanDetail.LastName = member.MemberLastName;
                        memberLoanDetail.DateOfBirth = member.MemberDateOfBirth;
                        memberLoanDetail.TotalLoans = totalLoan;
                        memberLoanDetail.Description = membershipCategory.MembershipCategoryDescription;
                        memberLoanDetailsDtos.Add(memberLoanDetail);
                    }
                }

            }
            List<MemberLoanDetailsDTO> order = memberLoanDetailsDtos.OrderBy(x => x.FirstName).ToList();
            ViewBag.DTOS = order;
            return View(order);
        }

        //Question No .12 (List of all Members who have not borrowed any DVD in the last 31 days)
        [Authorize(Roles = "Manager, Assistant")]
        /// <summary>
        /// It gets all the members, then gets all the loans for each member, then gets the loans that
        /// are over 31 days old, then gets the DVDCopy for each loan, then gets the DVDTitle for each
        /// DVDCopy, then gets the title name for each DVDTitle, then creates a MemberWithNoLoanDTO
        /// object for each loan that is over 31 days old, then adds the MemberWithNoLoanDTO object to a
        /// list of MemberWithNoLoanDTO objects, then returns the list of MemberWithNoLoanDTO objects to
        /// the view
        /// </summary>
        /// <returns>
        /// A list of members who have not borrowed a DVD for 31 days.
        /// </returns>
        public IActionResult MemberWithNoLoanFor31Days ()
        {
            List<Member> members = new List<Member>();
            members = applicationDbContext.Member.ToList();
            List<MemberWithNoLoanDTO> memberWithNoLoanDtos = new List<MemberWithNoLoanDTO>();
            List<Loan> loans = new List<Loan>();
            DVDCopy dvdCopy = new DVDCopy();
            Loan loan = new Loan();
            string title = "";
            foreach (var member in members)
        {
                loans = applicationDbContext.Loan.Include(x => x.DVDCopy).Where(x => x.Member == member).ToList();
                var l = loans.Where(x => (DateTime.Now.Date - x.DateOut.Date).TotalDays > 31).ToList();
                foreach (var memberLoan in l)
                {
                    dvdCopy = applicationDbContext.DVDCopy.Include(x => x.DVDTitle).Where(x => x.CopyNumber == memberLoan.DVDCopy.CopyNumber).First();
                    loan = memberLoan;
                    var dvdtitles = applicationDbContext.DVDTitle.Where(x => x.DVDNumber == dvdCopy.DVDTitle.DVDNumber);
                    foreach (var dvdTitle in dvdtitles)
                    {
                        title = dvdTitle.TitleName;
                    }
                }

                if (l.Count > 0)
                {
                    MemberWithNoLoanDTO memberWithNoLoan = new MemberWithNoLoanDTO();
                    memberWithNoLoan.FirstName = member.MemberFirstName;
                    memberWithNoLoan.LastName = member.MemberLastName;
                    memberWithNoLoan.Address = member.MemberAddress;
                    memberWithNoLoan.DVDTitle = title;
                    memberWithNoLoan.DateOut = loan.DateOut.Date.ToLongDateString();
                    memberWithNoLoan.NumberOfDays = (DateTime.Now.Date - loan.DateOut).TotalDays;
                    memberWithNoLoanDtos.Add(memberWithNoLoan);
                }
            }

            return View(memberWithNoLoanDtos);
        }






















    }

}
