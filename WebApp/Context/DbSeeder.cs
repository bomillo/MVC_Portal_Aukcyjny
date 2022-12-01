using Microsoft.AspNetCore.Identity;
using System.Text;
using WebApp.Models;
using WebApp.Services;

namespace WebApp.Context
{
    public class DbSeeder
    {
        //TODO: improve user password with hasher
        //Seeder nie zrobiony dla stawek zamiany walut i kluczy api 
        private readonly PortalAukcyjnyContext _dbContext;
        private readonly UsersService _userService;

        public DbSeeder(PortalAukcyjnyContext context, UsersService userService)
        {
            this._dbContext = context;
            _userService = userService;
        }

        public void Seed()
        {
            if (_dbContext.Database.CanConnect())
            {

                if (!_dbContext.Companies.Any())
                {
                    _dbContext.Companies.AddRange(GetCompanies());
                    _dbContext.SaveChanges();
                }
                if (!_dbContext.Users.Any())
                {
                    _dbContext.Users.AddRange(GetUsers());
                    _dbContext.SaveChanges();
                }

                if (!_dbContext.Categories.Any())
                {
                    _dbContext.Categories.AddRange(GetCategories(true));
                    _dbContext.SaveChanges();
                    _dbContext.Categories.AddRange(GetCategories(false));
                    _dbContext.SaveChanges();
                    _dbContext.Categories.AddRange(GetCategories(false));
                    _dbContext.SaveChanges();
                    _dbContext.Categories.AddRange(GetCategories(false));
                    _dbContext.SaveChanges();
                }
                if (!_dbContext.Products.Any())
                {
                    _dbContext.Products.AddRange(GetProducts());
                    _dbContext.SaveChanges();
                }


                if (!_dbContext.Auctions.Any())
                {
                    _dbContext.Auctions.AddRange(GetAuctions());
                    _dbContext.SaveChanges();
                }
                if (!_dbContext.AuctionAlerts.Any())
                {
                    _dbContext.AuctionAlerts.AddRange(GetAuctionAlerts());
                    _dbContext.SaveChanges();
                }
                if (!_dbContext.AuctionQuestion.Any())
                {
                    _dbContext.AuctionQuestion.AddRange(GetAuctionQuestions());
                    _dbContext.SaveChanges();
                }

                if (!_dbContext.ObservedAuctions.Any())
                {
                    _dbContext.ObservedAuctions.AddRange(GetObservedAuctions());
                    _dbContext.SaveChanges();
                }

                if (!_dbContext.DirectMessages.Any())
                {
                    _dbContext.DirectMessages.AddRange(GetDirectMessages());
                    _dbContext.SaveChanges();
                }

                if (!_dbContext.AuctionEditHistoryEntry.Any())
                {
                    _dbContext.AuctionEditHistoryEntry.AddRange(GetAuctionEditHistoryEntries());
                    _dbContext.SaveChanges();
                }

                if (!_dbContext.Bid.Any())
                {
                    _dbContext.Bid.AddRange(GetBids());
                    _dbContext.SaveChanges();
                }
            }
        }

        private IEnumerable<Company> GetCompanies()
        {
            Random random = new Random();
            List<Company> companies = new List<Company>();

            int howMany = random.Next(100, 200);

            for(int i = 0; i < howMany; i++)
            {

                Company company = new Company()
                {
                    Name = rndText(5, 50),
                    Email = rndText(5, 20, false) + "@" + rndEmailEnding(),
                    NIP = rndNIP(),
                };


                companies.Add(company);
            }


            return companies;
        }

        private IEnumerable<User> GetUsers()
        {
            Random random = new Random();
            List<User> users = new List<User>();
            var companies = _dbContext.Companies.ToList();
            int howMany = random.Next(800, 1000);

            for(int i = 0; i < howMany; i++)
            {
                User user = new User()
                {
                    Name = rndText(5, 99),
                    Email = rndText(5, 20, false) + "@" + rndEmailEnding(),
                    PasswordHashed = rndText(10, 30),
                    UserType = (UserType)random.Next(0, 2),
                    ThemeType = (ThemeType)random.Next(0, 2),
                    Language = (Language)random.Next(0, 2)
                };

                if(user.UserType != UserType.Admin)
                {
                    if (random.Next(2) == 1)
                    {
                        int quantity = _dbContext.Companies.Count();
                        user.Company = companies.ElementAt(random.Next(quantity - 1));
                        user.CompanyId = user.Company.CompanyId;
                    }
                }

                users.Add(user);
            }

            users.Add(new User()
            {
                Name = "admin",
                Email = "admin",
                PasswordHashed = _userService.HashPassword("admin"),
                UserType = UserType.Admin,
                ThemeType = ThemeType.Dark,
                Language = Language.FR
            });

            return users;
        }

        private IEnumerable<Category> GetCategories(bool firstStep)
        {
            Random random = new Random();

            int howMany = random.Next(50, 150);

            List<Category> categories = new List<Category>();

            List<Category> existingCategories = new List<Category>();

            if (!firstStep)
            {
                existingCategories = _dbContext.Categories.ToList();
            }

            for(int i = 0; i < howMany; i++)
            {
                Category category = new Category()
                {
                    Name = rndText(10, 40)
                };

                if (!firstStep)
                {
                    category.ParentCategory = existingCategories.ElementAt(random.Next(existingCategories.Count - 1));
                    category.ParentCategoryId = category.ParentCategory.CategoryId;
                }
                categories.Add(category);
            }

            return categories;
        }

        private IEnumerable<Product> GetProducts()
        {
            Random random = new Random();
            List<Product> products = new List<Product>();

            int howMany = random.Next(800, 1000);
            List<Category> categories = _dbContext.Categories.ToList();

            for(int i = 0; i < howMany; i++)
            {
                var category = categories.ElementAt(random.Next(categories.Count - 1));
                Product product = new Product()
                {
                    Category = category,
                    CategoryId = category.CategoryId,
                    Name = rndText(10, 40),
                    VatRate = rndVatRate(),
                    IsVatExcluded = false
                };

                if(product.VatRate == -1)
                {
                    product.IsVatExcluded = true;
                }

                products.Add(product);
            }


            return products;
        }

        private IEnumerable<Auction> GetAuctions()
        {
            Random random = new Random();
            List<Auction> auctions = new List<Auction>();

            List<User> users = _dbContext.Users.ToList();
            List<Product> products = _dbContext.Products.ToList();

            int howMany = random.Next(2000, 2500);

            for(int i = 0; i < howMany; i++)
            {
                var product = products.ElementAt(random.Next(products.Count - 1));
                var user = users.ElementAt(random.Next(users.Count - 1));
                Auction auction = new Auction()
                {
                    Description = rndText(100, 1999),
                    Title = rndText(10, 100),
                    IsDraft = Convert.ToBoolean(random.Next(2)),
                    OwnerId = user.UserId,
                    Owner = user,
                    CreationTime = DateTime.UtcNow.AddDays(random.Next(-200, 200)).AddHours(random.Next(24)).AddMinutes(random.Next(60)),
                    ProductId = product.ProductId,
                    Product = product
                };

                if (!auction.IsDraft)
                {
                    
                    auction.PublishedTime = DateTime.UtcNow.AddDays(random.Next(-200, 0)).AddHours(random.Next(24)).AddMinutes(random.Next(60));
                    auction.EndTime = auction.PublishedTime.Value.AddDays(random.Next(20)).AddHours(random.Next(24)).AddMinutes(random.Next(60));
                }

                auctions.Add(auction);
            }

            return auctions;
        }

        private IEnumerable<AuctionAlerts> GetAuctionAlerts()
        {
            Random random = new Random();
            var auctions = new List<AuctionAlerts>();

            var products = _dbContext.Products.ToList();
            var categories = _dbContext.Categories.ToList();
            var users = _dbContext.Users.Where(x => x.UserType != UserType.Admin).ToList();

            foreach(var user in users)
            {
                var howMany = random.Next(20);
                for(int i = 0; i < howMany; i++)
                {
                    var auction = new AuctionAlerts()
                    {
                        MaxPrice = random.NextDouble() * random.Next(100, 1000),
                        User = user,
                        UserId = user.UserId
                    };
                    if(random.Next(2) == 1)
                    {
                        auction.Product = products.ElementAt(random.Next(products.Count - 1));
                        auction.ProductId = auction.Product.ProductId;
                    }
                    else
                    {
                        auction.Category = categories.ElementAt(random.Next(categories.Count - 1));
                        auction.CategoryId = auction.Category.CategoryId;
                    }
                    auctions.Add(auction);
                }
            }


            return auctions;
        }

        private IEnumerable<AuctionQuestion> GetAuctionQuestions()
        {
            Random random = new Random();
            var auctions = _dbContext.Auctions.Where(x => x.IsDraft == false).ToList();
            var users = _dbContext.Users.ToList();
            var auctionQuestions = new List<AuctionQuestion>();
            int howMany = random.Next(500, 1000);

            for(int i = 0; i < howMany; i++)
            {
                var auction = auctions.ElementAt(random.Next(auctions.Count - 1));
                var user = users.ElementAt(random.Next(users.Count - 1));
                while(user.UserId == auction.OwnerId)
                {
                    user = users.ElementAt(random.Next(users.Count - 1));
                }
                var auctionQuestion = new AuctionQuestion()
                {
                    User = user,
                    UserId = user.UserId,
                    Auction = auction,
                    AuctionId = auction.AuctionId,
                    Question = rndText(100, 1200),
                    PublishedTime = auction.PublishedTime.Value.AddDays(random.Next(0, 20)),
                };

                if(random.Next(2) == 1)
                {
                    auctionQuestion.Answer = rndText(100, 1200);
                    auctionQuestion.AnsweredTime = auctionQuestion.PublishedTime.AddHours(random.Next(1, 48));
                }

                auctionQuestions.Add(auctionQuestion);
            }

            return auctionQuestions;
        }
      
        private IEnumerable<ObservedAuction> GetObservedAuctions()
        {
            Random random = new Random();

            var auctions = _dbContext.Auctions.ToList();
            var users = _dbContext.Users.ToList();
            var observedAuctions = new List<ObservedAuction>();
            foreach(var user in users)
            {
                int howMany = random.Next(1, 20);

                for(int i = 0; i < howMany; i++)
                {
                    var auction = auctions.ElementAt(random.Next(auctions.Count() - 1));
                    ObservedAuction observedAuction = new ObservedAuction()
                    {
                        UserId = user.UserId,
                        User = user,
                        Auction = auction,
                        AuctionId = auction.AuctionId
                    };
                    if(!observedAuctions.Any(x => x.AuctionId == observedAuction.AuctionId && x.UserId == observedAuction.UserId))
                    {
                        observedAuctions.Add(observedAuction);
                    }
                }
            }

            return observedAuctions;
        }

        private IEnumerable<DirectMessage> GetDirectMessages()
        {
            Random random = new Random();
            var messages = new List<DirectMessage>();
            var users = _dbContext.Users.ToList();
            
            int howMany = random.Next(3000, 5400);

            for (int i = 0; i < howMany; i++)
            {
                var sender = users.ElementAt(random.Next(users.Count - 1));
                var receiver = users.ElementAt(random.Next(users.Count - 1));

                var message = new DirectMessage()
                {
                    Sender = sender,
                    SenderId = sender.UserId,
                    Receiver = receiver,
                    ReceiverId = receiver.UserId,
                    Message = rndText(50, 1900),
                    SentTime = DateTime.UtcNow.AddDays(random.Next(-200, 200)).AddHours(random.Next(24)).AddMinutes(random.Next(60))
                };

                messages.Add(message);
            }

            for (int i = 0; i < 10; i++)
            {
                var sender = users.ElementAt(users.Count - 1);
                var receiver = users.ElementAt(random.Next(users.Count - 1));

                var message = new DirectMessage()
                {
                    Sender = sender,
                    SenderId = sender.UserId,
                    Receiver = receiver,
                    ReceiverId = receiver.UserId,
                    Message = rndText(50, 1900),
                    SentTime = DateTime.UtcNow.AddDays(random.Next(-200, 200)).AddHours(random.Next(24)).AddMinutes(random.Next(60))
                };

                messages.Add(message);
            }

            for (int i = 0; i < 10; i++)
            {
                var receiver = users.ElementAt(users.Count - 1);
                var sender = users.ElementAt(random.Next(users.Count - 1));

                var message = new DirectMessage()
                {
                    Sender = sender,
                    SenderId = sender.UserId,
                    Receiver = receiver,
                    ReceiverId = receiver.UserId,
                    Message = rndText(50, 1900),
                    SentTime = DateTime.UtcNow.AddDays(random.Next(-200, 200)).AddHours(random.Next(24)).AddMinutes(random.Next(60))
                };

                messages.Add(message);
            }

            return messages;

        }
        private IEnumerable<AuctionEditHistoryEntry> GetAuctionEditHistoryEntries()
        {
            Random rand = new Random();
            var auctionEditHistoryEntries = new List<AuctionEditHistoryEntry>();

            var auctions = _dbContext.Auctions.ToList();
            var users = _dbContext.Users.ToList();
            var howMany = rand.Next(700, 1000);

            for(int i = 0; i < howMany; i++)
            {
                var auction = auctions.ElementAt(rand.Next(auctions.Count - 1));

                var AuctionEditHistoryEntry = new AuctionEditHistoryEntry()
                {
                    Auction = auction,
                    AuctionId = auction.AuctionId,
                    User = auction.Owner,
                    UserId = auction.OwnerId,
                    Type = (ChangeType)rand.Next(2),
                    ChangedTime = auction.CreationTime.AddDays(rand.Next(19)).AddHours(rand.Next(24)).AddMinutes(rand.Next(60)),
                };

                if(AuctionEditHistoryEntry.Type == ChangeType.Desc)
                {
                    AuctionEditHistoryEntry.JsonChange = rndText(50, 1999) + " | " + auction.Description;
                }
                else if(AuctionEditHistoryEntry.Type == ChangeType.Name)
                {
                    AuctionEditHistoryEntry.JsonChange = rndText(10, 90) + " | " + auction.Title;
                }

                auctionEditHistoryEntries.Add(AuctionEditHistoryEntry);
            }

            return auctionEditHistoryEntries;
        }

        private IEnumerable<Bid> GetBids() 
        {
            Random random = new Random();
            var bids = new List<Bid>();
            var auctions = _dbContext.Auctions.Where(x => x.IsDraft == false).ToList();
            var users = _dbContext.Users.ToList();

            int howMany = random.Next(1000, 2000);

            for(int i = 0; i < howMany; i++)
            {
                var auction = auctions.ElementAt(random.Next(auctions.Count() - 1));
                var user = users.ElementAt(random.Next(users.Count() - 1));
                while(user.UserId == auction.Owner.UserId)
                {
                    user = users.ElementAt(random.Next(users.Count() - 1));
                }

                var currentBids = bids.Where(x => x.AuctionId == auction.AuctionId);
                var highestBid = currentBids.Select(x => x.Price).DefaultIfEmpty(0).Max();

                var bid = new Bid()
                {
                    UserId = user.UserId,
                    User = user,
                    Auction = auction,
                    AuctionId = auction.AuctionId
                };

                bid.Price = highestBid +  random.Next(50) + random.NextDouble();
                if (highestBid != 0)
                {
                    bid.BidTime = currentBids.Where(x => x.Price == highestBid).Select(x => x.BidTime).Max().AddMinutes(random.Next(40));
                }
                else
                {
                    bid.BidTime = auction.PublishedTime.Value.AddMinutes(random.Next(40));
                }

                bids.Add(bid);

            }

            return bids;
        }

        private string rndNIP()
        {
            Random random = new Random();
            StringBuilder str = new StringBuilder();
            str.Append(random.Next(1, 9).ToString());
            for(int i = 0; i < 9; i++)
            {
                str.Append(random.Next(0, 9).ToString());
            }


            return str.ToString();
        }

        private string rndText(int minLength, int maxLength, bool whiteSpaceEnabled = true)
        {
            Random rand = new Random();

            int length = rand.Next(minLength, maxLength);
            int maxVal = 122;
            if (whiteSpaceEnabled)
            {
                maxVal = 130;
            }

            StringBuilder str = new StringBuilder();

            for (int i = 0; i < length; i++)
            {

                int value = rand.Next(97, maxVal);
                if(value >= 123)
                {
                    str.Append(' ');
                }
                else
                {
                    str.Append((char)value);
                }
            }

            var result = str.ToString();
            result = char.ToUpper(str[0]) + result.Substring(1);
            return result;
        }

        private string rndEmailEnding()
        {
            Random rnd = new Random();
            var opt = rnd.Next(5);
            if(opt == 0) { return "gmail.com"; }
            else if(opt == 1) { return "outlook.com"; }
            else if(opt == 2) { return "onet.pl"; }
            else if(opt == 3) { return "o2.pl"; }
            else if(opt == 4) { return "wp.pl"; }
            else { return "yahoo.com"; }
        }

        private double rndVatRate()
        {
            Random rnd = new Random();
            var opt = rnd.Next(4);
            if (opt == 0) { return 23.00; }
            else if (opt == 1) { return 0.00; }
            else if (opt == 2) { return 5.00; }
            else if (opt == 3) { return 8.00; }
            else { return -1; }

        }
    }
}
