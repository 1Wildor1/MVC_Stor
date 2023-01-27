$(function () {

    /*Confirm page deletion*/

    $("a.delete").click(function () {
        if (!confirm("Confirm page deletion")) return false;
    });

    /* _______________________________________________________________________*/

    /*Sorting script*/

    $("table#pages tbody").sortable({
        items: "tr:not(.home)",
        Placeholder: "ui-state-highlight",
        update: function () {
            var ids = $("table#pages tbody").sortable("serialize");
            var url = "/Admin/pages/ReorderPages";

            $.post(url, ids, function (data) {
            });
        }
    });
});