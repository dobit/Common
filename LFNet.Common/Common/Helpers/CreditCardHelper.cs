using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using LFNet.Common.Extensions;

namespace LFNet.Common.Helpers
{
	public class CreditCardHelper
	{
		public static Regex _cardRegex = new Regex("^(?:(?<Visa>4\\d{3})|(?<MasterCard>5[1-5]\\d{2})|(?<Discover>6011)|(?<DinersClub>(?:3[68]\\d{2})|(?:30[0-5]\\d))|(?<Amex>3[47]\\d{2}))([ -]?)(?(DinersClub)(?:\\d{6}\\1\\d{4})|(?(Amex)(?:\\d{6}\\1\\d{5})|(?:\\d{4}\\1\\d{4}\\1\\d{4})))$", RegexOptions.Compiled);
		private static readonly string[] _visaCards = new string[]
		{
			"4111111111111111",
			"4012888888881881",
			"4007000000027",
			"4012888818888"
		};
		private static readonly string[] _masterCards = new string[]
		{
			"5555555555554444",
			"5105105105105100",
			"5424000000000015"
		};
		private static readonly string[] _amexCards = new string[]
		{
			"378282246310005",
			"371449635398431",
			"378734493671000",
			"370000000000002"
		};
		private static readonly string[] _discoverCards = new string[]
		{
			"6011111111111117",
			"6011000990139424",
			"6011000000000012"
		};
		private static readonly string[] _dinersClubCards = new string[]
		{
			"30569309025904",
			"38520000023237",
			"38000000000006"
		};
		private static readonly string[] _jcbCards = new string[]
		{
			"30569309025904",
			"38520000023237",
			"38000000000006"
		};
		private static List<string> _allCards;
		private static Dictionary<CreditCardType, string[]> _allCardsByType;
		public static string GetSecureCardNumber(string cardNumber)
		{
			if (string.IsNullOrEmpty(cardNumber) || cardNumber.StartsWith("XXXX"))
			{
				return cardNumber;
			}
			cardNumber = cardNumber.Replace(" ", "").Replace("-", "");
			return "XXXX" + cardNumber.Substring(cardNumber.Length - 4);
		}
		public static bool IsValidNumber(string cardNum)
		{
			CreditCardType? cardTypeFromNumber = CreditCardHelper.GetCardTypeFromNumber(cardNum);
			return CreditCardHelper.IsValidNumber(cardNum, cardTypeFromNumber);
		}
		public static bool IsValidNumber(string cardNum, CreditCardType? cardType)
		{
			return CreditCardHelper._cardRegex.Match(cardNum).Groups[cardType.ToString()].Success && CreditCardHelper.PassesLuhnTest(cardNum);
		}
		public static CreditCardType? GetCardTypeFromNumber(string cardNum)
		{
			GroupCollection groups = CreditCardHelper._cardRegex.Match(cardNum).Groups;
			if (groups[CreditCardType.Amex.ToString()].Success)
			{
				return new CreditCardType?(CreditCardType.Amex);
			}
			if (groups[CreditCardType.MasterCard.ToString()].Success)
			{
				return new CreditCardType?(CreditCardType.MasterCard);
			}
			if (groups[CreditCardType.Visa.ToString()].Success)
			{
				return new CreditCardType?(CreditCardType.Visa);
			}
			if (groups[CreditCardType.Discover.ToString()].Success)
			{
				return new CreditCardType?(CreditCardType.Discover);
			}
			if (groups[CreditCardType.DinersClub.ToString()].Success)
			{
				return new CreditCardType?(CreditCardType.DinersClub);
			}
			return null;
		}
		public static bool IsTestCardNumber(string cardNumber)
		{
			if (CreditCardHelper._allCards == null)
			{
				CreditCardHelper._allCards = new List<string>();
				CreditCardHelper._allCards.AddRange(CreditCardHelper._visaCards);
				CreditCardHelper._allCards.AddRange(CreditCardHelper._masterCards);
				CreditCardHelper._allCards.AddRange(CreditCardHelper._amexCards);
				CreditCardHelper._allCards.AddRange(CreditCardHelper._discoverCards);
				CreditCardHelper._allCards.AddRange(CreditCardHelper._dinersClubCards);
				CreditCardHelper._allCards.AddRange(CreditCardHelper._jcbCards);
			}
			cardNumber = cardNumber.Replace("-", "").Replace(" ", "");
			return CreditCardHelper._allCards.Contains(cardNumber);
		}
		public static string GetRandomCardTestNumber()
		{
			if (CreditCardHelper._allCards == null)
			{
				CreditCardHelper._allCards = new List<string>();
				CreditCardHelper._allCards.AddRange(CreditCardHelper._visaCards);
				CreditCardHelper._allCards.AddRange(CreditCardHelper._masterCards);
				CreditCardHelper._allCards.AddRange(CreditCardHelper._amexCards);
			}
			return CreditCardHelper._allCards.TakeRandom(1).First<string>();
		}
		public static string GetRandomCardTestNumber(CreditCardType cardType)
		{
			if (CreditCardHelper._allCardsByType == null)
			{
				CreditCardHelper._allCardsByType = new Dictionary<CreditCardType, string[]>();
				CreditCardHelper._allCardsByType.Add(CreditCardType.Visa, CreditCardHelper._visaCards);
				CreditCardHelper._allCardsByType.Add(CreditCardType.MasterCard, CreditCardHelper._masterCards);
				CreditCardHelper._allCardsByType.Add(CreditCardType.Amex, CreditCardHelper._amexCards);
				CreditCardHelper._allCardsByType.Add(CreditCardType.Discover, CreditCardHelper._discoverCards);
				CreditCardHelper._allCardsByType.Add(CreditCardType.DinersClub, CreditCardHelper._dinersClubCards);
				CreditCardHelper._allCardsByType.Add(CreditCardType.JCB, CreditCardHelper._jcbCards);
			}
			return CreditCardHelper._allCardsByType[cardType].TakeRandom(1).First<string>();
		}
		public static bool PassesLuhnTest(string cardNumber)
		{
			cardNumber = cardNumber.Replace("-", "").Replace(" ", "");
			int[] array = new int[cardNumber.Length];
			for (int i = 0; i < cardNumber.Length; i++)
			{
				array[i] = int.Parse(cardNumber.Substring(i, 1));
			}
			int num = 0;
			bool flag = false;
			for (int j = array.Length - 1; j >= 0; j--)
			{
				int num2 = array[j];
				if (flag)
				{
					num2 *= 2;
					if (num2 > 9)
					{
						num2 -= 9;
					}
				}
				num += num2;
				flag = !flag;
			}
			return num % 10 == 0;
		}
	}
}
