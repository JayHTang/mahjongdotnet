$(document).ready(function () {
    $('#result_form').submit(function (e) {
        var winner = $('#result_form #WinnerId').val();
        var zimo = $('#result_form #Zimo').is(':checked');
        var dianpao = $('#result_form #DianpaoId').val();
        var chengbao = $('#result_form #ChengbaoId').val();
        var qianggang = $('#result_form #Qianggang').is(':checked');
        var gangkai = $('#result_form #Gangkai').is(':checked');
        var hand = $('#result_form #HandId').val();

        $(".alert").remove();
        if (winner != 0) {
            if (!zimo && dianpao == 0) {
                $('#result_form #Zimo').after('<span class="alert alert-danger">必须自摸或点炮</span>');
                $('#result_form #DianpaoId').after('<span class="alert alert-danger">必须自摸或点炮</span>');
                e.preventDefault();
            }
            else if (zimo && qianggang && !gangkai) {
                $('#result_form #Gangkai').after('<span class="alert alert-danger">送杠必须杠开</span>');
                e.preventDefault();
            }
            else if (!zimo && qianggang && gangkai){
                $('#result_form #Gangkai').after('<span class="alert alert-danger">抢杠不算杠开</span>');
                e.preventDefault();
            }
            else if (zimo && dianpao != 0 && !qianggang) {
                $('#result_form #Zimo').after('<span class="alert alert-danger">不可同时自摸和点炮</span>');
                $('#result_form #DianpaoId').after('<span class="alert alert-danger">不可同时自摸和点炮</span>');
                e.preventDefault();
            }
            else if (winner == dianpao) {
                $('#result_form #WinnerId').after('<span class="alert alert-danger">不可点自己炮</span>');
                $('#result_form #DianpaoId').after('<span class="alert alert-danger">不可点自己炮</span>');
                e.preventDefault();
            }
            else if (winner == chengbao) {
                $('#result_form #WinnerId').after('<span class="alert alert-danger">不可承包自己</span>');
                $('#result_form #ChengbaoId').after('<span class="alert alert-danger">不可承包自己</span>');
                e.preventDefault();
            }
            else if (qianggang && dianpao == 0) {
                $('#result_form #Qianggang').after('<span class="alert alert-danger">抢杠必须有人点炮</span>');
                $('#result_form #DianpaoId').after('<span class="alert alert-danger">抢杠必须有人点炮</span>');
                e.preventDefault();
            }
            else if (hand == 0) {
                $('#result_form #HandId').after('<span class="alert alert-danger">请选择牌型</span>');
                e.preventDefault();
            }
        }

    });

    $('#edit_form').submit(function (e) {
        var winner = $('#edit_form #WinnerId').val();
        var zimo = $('#edit_form #Zimo').is(':checked');
        var dianpao = $('#edit_form #DianpaoId').val();
        var chengbao = $('#edit_form #ChengbaoId').val();
        var qianggang = $('#edit_form #Qianggang').is(':checked');
        var gangkai = $('#edit_form #Gangkai').is(':checked');
        var hand = $('#edit_form #HandId').val();

        $(".alert").remove();
        if (winner != 0) {
            if (!zimo && dianpao == 0) {
                $('#edit_form #Zimo').after('<span class="alert alert-danger">必须自摸或点炮</span>');
                $('#edit_form #DianpaoId').after('<span class="alert alert-danger">必须自摸或点炮</span>');
                e.preventDefault();
            }
            else if (zimo && qianggang && !gangkai) {
                $('#edit_form #Gangkai').after('<span class="alert alert-danger">送杠必须杠开</span>');
                e.preventDefault();
            }
            else if (!zimo && qianggang && gangkai) {
                $('#edit_form #Gangkai').after('<span class="alert alert-danger">抢杠不算杠开</span>');
                e.preventDefault();
            }
            else if (zimo && dianpao != 0 && !qianggang) {
                $('#edit_form #Zimo').after('<span class="alert alert-danger">不可同时自摸和点炮</span>');
                $('#edit_form #DianpaoId').after('<span class="alert alert-danger">不可同时自摸和点炮</span>');
                e.preventDefault();
            }
            else if (winner == dianpao) {
                $('#edit_form #WinnerId').after('<span class="alert alert-danger">不可点自己炮</span>');
                $('#edit_form #DianpaoId').after('<span class="alert alert-danger">不可点自己炮</span>');
                e.preventDefault();
            }
            else if (winner == chengbao) {
                $('#edit_form #WinnerId').after('<span class="alert alert-danger">不可承包自己</span>');
                $('#edit_form #ChengbaoId').after('<span class="alert alert-danger">不可承包自己</span>');
                e.preventDefault();
            }
            else if (qianggang && dianpao == 0) {
                $('#edit_form #Qianggang').after('<span class="alert alert-danger">抢杠必须有人点炮</span>');
                $('#edit_form #DianpaoId').after('<span class="alert alert-danger">抢杠必须有人点炮</span>');
                e.preventDefault();
            }
            else if (hand == 0) {
                $('#edit_form #HandId').after('<span class="alert alert-danger">请选择牌型</span>');
                e.preventDefault();
            }
        }

    });
});