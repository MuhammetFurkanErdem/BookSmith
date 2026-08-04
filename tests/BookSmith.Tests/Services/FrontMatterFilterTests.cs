using System.Collections.Generic;
using Xunit;
using BookSmith.Services.Cleaning;

namespace BookSmith.Tests.Services;

public class FrontMatterFilterTests
{
    private FrontMatterFilter CreateFilter() => new FrontMatterFilter();

    // ──────────────────────────────────────────────────────────────────
    // Helper: build a realistic Turkish copyright page
    private const string TurkishCopyrightPage =
        "PEGASUS YAYINLARI\n" +
        "Bu kitabın tüm hakları saklıdır.\n" +
        "Yayın Koordinatörü: Ahmet Yılmaz\n" +
        "Çeviren: Mehmet Kaya\n" +
        "Kapak Tasarımı: Grafik Atölye\n" +
        "ISBN: 978-605-123-456-7\n" +
        "Sertifika No: 12345\n" +
        "3. Baskı, Ocak 2023\n";

    private const string InternationalCopyrightPage =
        "First published in the United Kingdom in 2020\n" +
        "Copyright © 2020 by the Author\n" +
        "All rights reserved. No part of this publication may be reproduced.\n" +
        "Published by Bloomsbury Publishing Plc\n" +
        "Library of Congress Cataloging-in-Publication Data\n" +
        "ISBN 978-1-4088-1234-5\n" +
        "Printed in the United States of America\n";

    private const string MainContentPage =
        "Geralt bir anlığına durdu. Kılıcının kabzasını sıkıca kavradı.\n" +
        "\"Yeniden mi?\" diye sordu. \"Kaçıncı sefer bu?\"\n" +
        "Ciri gülümsedi ama gözleri ciddi kaldı.\n" +
        "\"Ama Kader Kılıcı diyorlar ya buna. Sen de benim kaderimsin, Geralt.\"\n" +
        "Geralt içini çekti ve sisi yaran kılıcını kaldırdı.\n" +
        "Yeniden başlıyordu. Her şey. Tüm döngü bir kez daha.\n" +
        "Bu sayfa yeterince uzun main content içeriyor, yayınevine dair bir şey yok.\n" +
        "Devam ediyoruz, hikaye ilerliyor, karakterler konuşuyor.\n";

    // ──────────────────────────────────────────────────────────────────

    [Fact]
    public void FilterFrontMatter_Removes_Turkish_Copyright_Page()
    {
        var filter = CreateFilter();
        var pages = new List<string> { TurkishCopyrightPage, MainContentPage };

        string result = filter.FilterFrontMatter(pages);

        Assert.DoesNotContain("PEGASUS", result);
        Assert.Contains("Geralt", result);
    }

    [Fact]
    public void FilterFrontMatter_Removes_International_Copyright_Page()
    {
        var filter = CreateFilter();
        var pages = new List<string> { InternationalCopyrightPage, MainContentPage };

        string result = filter.FilterFrontMatter(pages);

        Assert.DoesNotContain("Bloomsbury", result);
        Assert.Contains("Geralt", result);
    }

    [Fact]
    public void FilterFrontMatter_Removes_Multiple_Front_Matter_Pages()
    {
        var filter = CreateFilter();
        var pages = new List<string>
        {
            TurkishCopyrightPage,
            InternationalCopyrightPage,
            MainContentPage,
            "İkinci içerik sayfası. Hikaye devam ediyor. Uzun içerik metni burada."
        };

        string result = filter.FilterFrontMatter(pages);

        Assert.DoesNotContain("PEGASUS", result);
        Assert.DoesNotContain("Bloomsbury", result);
        Assert.Contains("Geralt", result);
    }

    [Fact]
    public void FilterFrontMatter_Does_Not_Remove_Main_Content()
    {
        var filter = CreateFilter();
        var pages = new List<string> { MainContentPage };

        string result = filter.FilterFrontMatter(pages);

        Assert.Contains("Geralt", result);
    }

    [Fact]
    public void FilterFrontMatter_Removes_Short_Pages_At_Start()
    {
        var filter = CreateFilter();
        // Very short page (title-page like) at start
        var pages = new List<string> { "Kader Kılıcı", MainContentPage };

        string result = filter.FilterFrontMatter(pages);

        Assert.Contains("Geralt", result);
    }

    [Fact]
    public void FilterFrontMatter_Does_Not_Touch_Pages_After_Scan_Limit()
    {
        var filter = CreateFilter();
        // First 8 pages are main content, copyright page is #9 (index 8)
        var pages = new List<string>();
        for (int i = 0; i < 8; i++)
            pages.Add(MainContentPage + $" Sayfa {i + 1}");
        pages.Add(TurkishCopyrightPage); // page 9 — beyond scan limit

        string result = filter.FilterFrontMatter(pages);

        // All main content should be present (first content page stops scanning)
        Assert.Contains("Geralt", result);
    }

    [Fact]
    public void FilterFrontMatter_Returns_Empty_For_Empty_Input()
    {
        var filter = CreateFilter();
        string result = filter.FilterFrontMatter(new List<string>());
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void FilterFrontMatter_Returns_Empty_For_Null_Input()
    {
        var filter = CreateFilter();
        string result = filter.FilterFrontMatter(null!);
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void FilterFrontMatter_Never_Removes_All_Content()
    {
        var filter = CreateFilter();
        // Even if all pages look like front matter, at least some content should be returned
        var pages = new List<string>
        {
            TurkishCopyrightPage,
            InternationalCopyrightPage,
            TurkishCopyrightPage,
        };

        string result = filter.FilterFrontMatter(pages);

        // Safety: should not return empty string (never discard everything)
        Assert.False(string.IsNullOrWhiteSpace(result));
    }

    [Fact]
    public void FilterFrontMatter_Handles_Single_Main_Content_Page()
    {
        var filter = CreateFilter();
        var pages = new List<string> { MainContentPage };

        string result = filter.FilterFrontMatter(pages);

        Assert.Contains("Geralt", result);
    }
}
