$(document).ready(function () {
    function appendValidationError(formSelector, fieldSelector, message) {
        var $field = $(formSelector + " " + fieldSelector);
        if (!$field.length) return;
        var $pill = $field.closest(".mahjong-check-item");
        var $error = $('<span class="validation-alert">' + message + '</span>');
        if ($pill.length) {
            $pill.append($error);
        } else {
            $field.after($error);
        }
    }

    function validateRoundForm(formSelector, e) {
        var winner = $(formSelector + " #WinnerId").val();
        var zimo = $(formSelector + " #Zimo").is(":checked");
        var dianpao = $(formSelector + " #DianpaoId").val();
        var chengbao = $(formSelector + " #ChengbaoId").val();
        var qianggang = $(formSelector + " #Qianggang").is(":checked");
        var gangkai = $(formSelector + " #Gangkai").is(":checked");
        var hand = $(formSelector + " #HandId").val();

        $(formSelector + " .validation-alert").remove();

        if (winner && winner != 0) {
            if (!zimo && (!dianpao || dianpao == 0)) {
                appendValidationError(formSelector, "#Zimo", "必须自摸或点炮");
                appendValidationError(formSelector, "#DianpaoId", "必须自摸或点炮");
                e.preventDefault();
            } else if (zimo && qianggang && !gangkai) {
                appendValidationError(formSelector, "#Gangkai", "送杠必须杠开");
                e.preventDefault();
            } else if (!zimo && qianggang && gangkai) {
                appendValidationError(formSelector, "#Gangkai", "抢杠不算杠开");
                e.preventDefault();
            } else if (zimo && dianpao && dianpao != 0 && !qianggang) {
                appendValidationError(formSelector, "#Zimo", "不可同时自摸和点炮");
                appendValidationError(formSelector, "#DianpaoId", "不可同时自摸和点炮");
                e.preventDefault();
            } else if (winner == dianpao) {
                appendValidationError(formSelector, "#WinnerId", "不可点自己炮");
                appendValidationError(formSelector, "#DianpaoId", "不可点自己炮");
                e.preventDefault();
            } else if (winner == chengbao) {
                appendValidationError(formSelector, "#WinnerId", "不可承包自己");
                appendValidationError(formSelector, "#ChengbaoId", "不可承包自己");
                e.preventDefault();
            } else if (qianggang && (!dianpao || dianpao == 0)) {
                appendValidationError(formSelector, "#Qianggang", "抢杠必须有人点炮");
                appendValidationError(formSelector, "#DianpaoId", "抢杠必须有人点炮");
                e.preventDefault();
            } else if (!hand || hand == 0) {
                appendValidationError(formSelector, "#HandId", "请选择牌型");
                e.preventDefault();
            }
        }
    }

    $("#result_form").on("submit", function (e) {
        validateRoundForm("#result_form", e);
    });

    $("#edit_form").on("submit", function (e) {
        validateRoundForm("#edit_form", e);
    });
});
