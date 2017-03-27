$(document).ready(function () {
    //debugger;
    var i = 1;
    $("#add_row_Backend").click(function () {
        debugger;
        $("table tr:second").clone().find("input").each(function () {
            $(this).val('').attr({
                'id': function (_, id) {
                    return id + i
                },
                'name': function (_, name) {
                    return name + i
                },
                'value': ''
            });
        }).end().appendTo("table");
        i++;
    });

    $(document).on('click', 'button.removebutton', function () {
        alert("aa");
        $(this).closest('tr').remove();
        return false;
    });
});