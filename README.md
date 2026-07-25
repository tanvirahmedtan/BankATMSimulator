# Bank / ATM Simulator (C# Console App)

একটা সম্পূর্ণ কাজ-করা (fully functional) C# কনসোল অ্যাপ, Object-Oriented ডিজাইন, JSON ফাইলে ডেটা সংরক্ষণ (persistence) সহ — যেন প্রতিবার অ্যাপ বন্ধ করে আবার চালালেও তোমার আগের অ্যাকাউন্ট আর ব্যালেন্স ঠিক থাকে।

## ফিচারসমূহ
- নতুন অ্যাকাউন্ট খোলা (নাম, ৪-ডিজিট PIN, ওপেনিং ডিপোজিট — মিনিমাম ৫০০)
- Auto-generated অ্যাকাউন্ট নম্বর (যেমন: `BD12345678`)
- Login (৩ বার ভুল PIN দিলে অ্যাকাউন্ট লক হয়ে যাবে)
- Balance Check
- Deposit / Withdraw
- এক অ্যাকাউন্ট থেকে আরেক অ্যাকাউন্টে Transfer
- Mini Statement (শেষ ১০টা transaction)
- PIN পরিবর্তন
- PIN কখনো plain text এ সেভ হয় না — SHA-256 দিয়ে hash করে রাখা হয়
- সব ডেটা `Data/accounts.json` ফাইলে সেভ থাকে

## প্রজেক্ট স্ট্রাকচার
```
BankATMSimulator/
├── BankATMSimulator.csproj
├── Program.cs                 → মেনু ও কনসোল UI
├── Models/
│   ├── Account.cs
│   └── Transaction.cs
├── Services/
│   └── BankService.cs         → সব বিজনেস লজিক (create/login/deposit/withdraw/transfer)
└── Data/
    └── accounts.json          → ডেটা এখানে সেভ হয় (প্রথমে খালি)
```

## কীভাবে রান করবে

### Visual Studio 2022 দিয়ে
1. `BankATMSimulator` ফোল্ডারটা আনজিপ করো।
2. `BankATMSimulator.csproj` ফাইলে ডাবল-ক্লিক করো — Visual Studio সরাসরি ওপেন করে নেবে।
3. উপরে সবুজ ▶ (Start / F5) বাটনে ক্লিক করো।

### `dotnet` CLI দিয়ে (VS Code বা টার্মিনাল থেকে)
```bash
cd BankATMSimulator
dotnet run
```
(তোমার মেশিনে .NET 8 SDK ইনস্টল থাকতে হবে — না থাকলে [dotnet.microsoft.com](https://dotnet.microsoft.com/download) থেকে নামিয়ে নাও)

## পরের ধাপে যা যোগ করতে পারো (পোর্টফোলিওর জন্য)
- Unit test (xUnit দিয়ে `BankService` টেস্ট করা)
- Interest calculation feature
- Admin panel (সব অ্যাকাউন্ট দেখা, লকড অ্যাকাউন্ট আনলক করা)
- Console UI থেকে বের হয়ে একটা ASP.NET Core Web API + simple frontend বানানো — এটা GitHub পোর্টফোলিওর জন্য অনেক বেশি ইমপ্রেসিভ হবে
