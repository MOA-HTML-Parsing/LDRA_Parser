//using LDRA_Parser.Model;

//public void compareBeforeAfter(IEnumerable<BeforeItem>? beforeItems, IEnumerable<AfterItem>? afterItems)
//{
//    List<BeforeItem> beforeit = new List<BeforeItem>();
//    List<AfterItem> afterit = new List<AfterItem>();

//    // BeforeItem과 AfterItem을 비교
//    foreach (var beforeItem in beforeItems)
//    {
//        bool matchFound = false;
//        foreach (var afterItem in afterItems)
//        {
//            if (beforeItem.LDRA_Code == afterItem.LDRA_Code)
//            {
//                ProcessMatchingItems(beforeItem, afterItem);
//                if (beforeItem.violationItems.Count > 0 || afterItem.violationItems.Count > 0)
//                {
//                    beforeit.Add(beforeItem);
//                    afterit.Add(afterItem);
//                }
//                matchFound = true;
//                break;
//            }
//        }

//        if (!matchFound)
//        {
//            ProcessNonMatchingBeforeItem(beforeItem, afterit);
//            beforeit.Add(beforeItem);
//        }
//    }

//    // AfterItem 기준으로도 비교
//    foreach (var afterItem in afterItems)
//    {
//        if (beforeItems.All(beforeItem => beforeItem.LDRA_Code != afterItem.LDRA_Code))
//        {
//            ProcessNonMatchingAfterItem(afterItem, beforeit);
//            afterit.Add(afterItem);
//        }
//    }

//    BeforeVM.updateBeforeList(beforeit);
//    AfterVM.updateAfterList(afterit);
//    parsedHLM.updateParsedHtmlList(beforeViolations);
//    parsedHLM.updateParsedHtmlList(afterViolations);
//}

//private void ProcessMatchingItems(BeforeItem beforeItem, AfterItem afterItem)
//{
//    popupHTMLPasing(beforeItem.HrefValue, afterItem.HrefValue);

//    while (true)
//    {
//        var beforeToRemove = new List<ViolationItem>();
//        var afterToRemove = new List<ViolationItem>();

//        foreach (var beforeViolationItem in beforeViolations)
//        {
//            bool isMatched = false;
//            foreach (var afterViolationItem in afterViolations)
//            {
//                if (beforeViolationItem.IsSame(afterViolationItem))
//                {
//                    isMatched = true;
//                    beforeToRemove.Add(beforeViolationItem);
//                    afterToRemove.Add(afterViolationItem);
//                    break;
//                }
//            }

//            if (!isMatched)
//            {
//                beforeItem.violationItems.Add(beforeViolationItem);
//                beforeToRemove.Add(beforeViolationItem);
//            }
//            else
//            {
//                break;
//            }
//        }

//        if (beforeToRemove.Count > 0)
//        {
//            RemoveViolations(beforeToRemove, afterToRemove);
//        }
//        else
//        {
//            break;
//        }
//    }

//    foreach (var afterViolationItem in afterViolations)
//    {
//        afterItem.violationItems.Add(afterViolationItem);
//    }
//}

//private void ProcessNonMatchingBeforeItem(BeforeItem beforeItem, List<AfterItem> afterit)
//{
//    popupHTMLPasing(beforeItem.HrefValue, beforeItem.HrefValue);
//    foreach (var beforeViolationItem in beforeViolations)
//    {
//        beforeItem.violationItems.Add(beforeViolationItem);
//    }
//    afterit.Add(null); // 칸 맞추기
//}

//private void ProcessNonMatchingAfterItem(AfterItem afterItem, List<BeforeItem> beforeit)
//{
//    popupHTMLPasing(afterItem.HrefValue, afterItem.HrefValue);
//    foreach (var afterViolationItem in afterViolations)
//    {
//        afterItem.violationItems.Add(afterViolationItem);
//    }
//    beforeit.Add(null); // 칸 맞추기
//}

//private void RemoveViolations(List<ViolationItem> beforeToRemove, List<ViolationItem> afterToRemove)
//{
//    foreach (var item in beforeToRemove)
//    {
//        beforeViolations.Remove(item);
//    }
//    if (afterToRemove.Count > 0)
//    {
//        foreach (var item in afterToRemove)
//        {
//            afterViolations.Remove(item);
//        }
//    }
//}