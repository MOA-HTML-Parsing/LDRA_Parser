//using ldra_parser.model;

//public void comparebeforeafter(ienumerable<beforeitem>? beforeitems, ienumerable<afteritem>? afteritems)
//{
//    list<beforeitem> beforeit = new list<beforeitem>();
//    list<afteritem> afterit = new list<afteritem>();

//    var afteritemdict = afteritems.todictionary(item => item.ldra_code);

//    foreach (var beforeitem in beforeitems)
//    {
//        if (afteritemdict.trygetvalue(beforeitem.ldra_code, out var afteritem))
//        {
//            processmatchingitems(beforeitem, afteritem);
//            if (beforeitem.violationitems.count > 0 || afteritem.violationitems.count > 0)
//            {
//                beforeit.add(beforeitem);
//                afterit.add(afteritem);
//            }
//        }
//        else
//        {
//            processnonmatchingbeforeitem(beforeitem, afterit);
//            beforeit.add(beforeitem);
//        }
//    }

//    foreach (var afteritem in afteritems)
//    {
//        if (beforeitems.all(beforeitem => beforeitem.ldra_code != afteritem.ldra_code))
//        {
//            processnonmatchingafteritem(afteritem, beforeit);
//            afterit.add(afteritem);
//        }
//    }

//    beforevm.updatebeforelist(beforeit);
//    aftervm.updateafterlist(afterit);
//    parsedhlm.updateparsedhtmllist(beforeviolations);
//    parsedhlm.updateparsedhtmllist(afterviolations);
//}

//private void processmatchingitems(beforeitem beforeitem, afteritem afteritem)
//{
//    popuphtmlpasing(beforeitem.hrefvalue, afteritem.hrefvalue);

//    while (true)
//    {
//        var beforetoremove = new list<violationitem>();
//        var aftertoremove = new list<violationitem>();

//        foreach (var beforeviolationitem in beforeviolations)
//        {
//            bool ismatched = false;
//            foreach (var afterviolationitem in afterviolations)
//            {
//                if (beforeviolationitem.issame(afterviolationitem))
//                {
//                    ismatched = true;
//                    beforetoremove.add(beforeviolationitem);
//                    aftertoremove.add(afterviolationitem);
//                    break;
//                }
//            }

//            if (!ismatched)
//            {
//                beforeitem.violationitems.add(beforeviolationitem);
//                beforetoremove.add(beforeviolationitem);
//            }
//        }

//        if (beforetoremove.count > 0)
//        {
//            removeviolations(beforetoremove, aftertoremove);
//        }
//        else
//        {
//            break;
//        }
//    }

//    foreach (var afterviolationitem in afterviolations)
//    {
//        afteritem.violationitems.add(afterviolationitem);
//    }
//}

//private void processnonmatchingbeforeitem(beforeitem beforeitem, list<afteritem> afterit)
//{
//    popuphtmlpasing(beforeitem.hrefvalue, beforeitem.hrefvalue);
//    foreach (var beforeviolationitem in beforeviolations)
//    {
//        beforeitem.violationitems.add(beforeviolationitem);
//    }
//    afterit.add(null); // 칸 맞추기
//}

//private void processnonmatchingafteritem(afteritem afteritem, list<beforeitem> beforeit)
//{
//    popuphtmlpasing(afteritem.hrefvalue, afteritem.hrefvalue);
//    foreach (var afterviolationitem in afterviolations)
//    {
//        afteritem.violationitems.add(afterviolationitem);
//    }
//    beforeit.add(null); // 칸 맞추기
//}

//private void removeviolations(list<violationitem> beforetoremove, list<violationitem> aftertoremove)
//{
//    foreach (var item in beforetoremove)
//    {
//        beforeviolations.remove(item);
//    }
//    foreach (var item in aftertoremove)
//    {
//        afterviolations.remove(item);
//    }
//}