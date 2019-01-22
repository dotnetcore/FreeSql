
(function (window) {

    window.base = function () { };

    base.prototype = {
        showLoading: function (obj) {
            var index = layer.msg(obj.msg, {
                icon: 16,
                shade: 0.1,
                shadeClose: false,
            });
            return index;
        },
        closeLoading: function (index) {
            layer.close(index);
        },
        showMessage: function (options) {
            if (layer == null) {
                alert(options.msg);
                return;
            }
            var yes = function (index) {
                if ($.isFunction(options.yes)) {
                    options.yes();
                }
                layer.close(index);
            };
            layer.alert(options.msg || "操作成功", {
                icon: options.type || 1,
                scrollbar: false,
                shadeClose: false,
                closeBtn: 0,
                skin: 'layui-layer-lan'//'layer-ext-moon'
            }, yes);
        },
        //options={title:"标题",msg:"内容",yes:function,no:function}
        showConfirm: function (options) {
            if (options == null || options.msg == null) {
                return;
            }
            var yes = options.yes;
            var no = options.no;
            var defaultAction = function (index) {
                layer.close(index);
            };
            if (yes == null) {
                yes = defaultAction;
            }
            if (no == null) {
                no = defaultAction
            }
            ////layer.confirm(options.msg, yes, options.title, no);
            //layer.confirm(options.msg, { btn: ['确定', '取消'] }, yes, no);
            layer.confirm(options.msg, {
                btn: ['确定', '取消'], //按钮
                icon: 3,
                shadeClose: false,
                skin: 'layer-ext-moon'
            }, yes, no);
        },
        ajax: function (url, appendPostData, beforeFn, completeFn, successFn, errorFn, isShowLoading) {
            jQuery.ajax({
                type: "POST",
                url: url,
                data: appendPostData,
                global: false,
                beforeSend: function (XMLHttpRequest) {
                    if (jQuery.isFunction(beforeFn)) {
                        if (beforeFn(XMLHttpRequest)) {
                            if (isShowLoading != false) {
                                freejs.showLoading();
                            }
                        }
                        else {
                            return false;
                        }
                    }
                    else {
                        if (isShowLoading != false) {
                            freejs.showLoading();
                        }
                    }
                },
                success: function (data, textStatus) {
                    if (jQuery.isFunction(successFn)) {
                        successFn(data, textStatus);
                    }
                },
                complete: function (XMLHttpRequest, textStatus) {
                    var gohome = XMLHttpRequest.getResponseHeader("Timeout");
                    if (gohome) {
                        // window.top.window.location.href = gohome;
                        return false;
                    }
                    if (isShowLoading != false) {
                        freejs.hideLoading();
                    }
                    if (jQuery.isFunction(completeFn)) {
                        completeFn();
                    }
                },
                error: function (e, d, s, u, b) {
                    if (jQuery.isFunction(errorFn)) {
                        errorFn(e, d, s);
                    }
                    else {
                        freejs.showMessage({
                            title: "发生异常",
                            type: 2,
                            msg: s
                        });
                    }
                }
            });
        }
    };

    window.freejs = new base();
})(window);