using System;
using System.Collections.Generic;
using System.Linq;
using BookSmith.Core.Interfaces;

namespace BookSmith.Services.Cleaning;

/// <summary>
/// Detects and removes publisher front-matter pages from the beginning of a book.
/// Scans only the first <see cref="MaxPagesToScan"/> pages to avoid touching main content.
/// </summary>
public class FrontMatterFilter : IFrontMatterFilter
{
    /// <summary>Maximum number of pages to inspect for front matter.</summary>
    private const int MaxPagesToScan = 8;

    /// <summary>Minimum keyword hits on a single page to classify it as front matter.</summary>
    private const int KeywordHitThreshold = 3;

    /// <summary>Pages shorter than this character count at the start are also treated as front matter candidates.</summary>
    private const int ShortPageThreshold = 200;

    /// <summary>Turkish publisher and front-matter keywords.</summary>
    private static readonly string[] TurkishKeywords =
    {
        "YAYINLARI", "YAYINCILIK", "YAYINEVI", "YAYINEVİ",
        "Baskı", "BASKI", "baskı",
        "ISBN", "isbn",
        "Sertifika No", "sertifika no", "SERTİFİKA NO",
        "Çeviren", "çeviren", "ÇEVİREN",
        "Çevirmen", "çevirmen", "ÇEVİRMEN",
        "Yayın Koordinatörü", "yayın koordinatörü",
        "Kapak Tasarımı", "kapak tasarımı",
        "Kapak Düzeni", "kapak düzeni",
        "Dizgi", "dizgi",
        "Baskı-Cilt", "baskı-cilt",
        "Bestseller", "bestseller",
        "Tüm hakları saklıdır", "tüm hakları saklıdır",
        "Tüm Hakları Saklıdır",
        "PEGASUS", "CAN YAYINLARI", "İTHAKİ", "ALTIN KİTAPLAR",
        "DOĞAN KİTAP", "EPSILON", "REMZI", "METİS",
        "Öykü", "Roman",
        "Orijinal adı", "orijinal adı",
        "Originally published", "originally published",
        "Türkçesi", "türkçesi",
        "Yayına Hazırlayan", "yayına hazırlayan",
        "Genel Yayın Yönetmeni", "genel yayın yönetmeni",
    };

    /// <summary>International publisher and copyright keywords.</summary>
    private static readonly string[] InternationalKeywords =
    {
        "All rights reserved", "all rights reserved",
        "First published", "first published",
        "Printed in", "printed in",
        "Library of Congress", "library of congress",
        "Copyright ©", "copyright ©", "Copyright (c)", "© copyright",
        "Published by", "published by",
        "Translation rights", "translation rights",
        "No part of this", "no part of this",
        "Cataloging-in-Publication", "cataloging-in-publication",
        "British Library", "british library",
        "Penguin", "HarperCollins", "Bloomsbury", "Simon & Schuster",
        "Random House", "Macmillan", "Hachette",
    };

    private static readonly HashSet<string> AllKeywords = new(
        TurkishKeywords.Concat(InternationalKeywords),
        StringComparer.OrdinalIgnoreCase);

    /// <inheritdoc />
    public string FilterFrontMatter(IReadOnlyList<string> pages)
    {
        if (pages == null || pages.Count == 0)
            return string.Empty;

        int pagesToInspect = Math.Min(MaxPagesToScan, pages.Count);
        int lastFrontMatterPage = -1;

        for (int i = 0; i < pagesToInspect; i++)
        {
            string page = pages[i] ?? string.Empty;

            if (IsFrontMatterPage(page, i))
            {
                lastFrontMatterPage = i;
            }
            else
            {
                // Stop at the first page that looks like main content
                break;
            }
        }

        // Build result from pages after the last detected front-matter page
        int startPage = lastFrontMatterPage + 1;
        if (startPage >= pages.Count)
            startPage = 0; // Safety: never discard everything

        return string.Join("\n\n", pages.Skip(startPage));
    }

    private bool IsFrontMatterPage(string pageText, int pageIndex)
    {
        if (string.IsNullOrWhiteSpace(pageText))
            return true;

        // Very short pages at the start are front-matter candidates
        if (pageText.TrimEnd().Length < ShortPageThreshold)
            return true;

        // Count keyword hits
        int hits = 0;
        foreach (string keyword in AllKeywords)
        {
            if (pageText.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            {
                hits++;
                if (hits >= KeywordHitThreshold)
                    return true;
            }
        }

        return false;
    }
}
