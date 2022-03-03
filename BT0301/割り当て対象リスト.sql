--- 雛形作成対象経線IDの抽出
with
calc_wireids as ( 
    select
        calc.wire_list_detail_id
        , calc.wire_id
        , calc.similar_point
        , calc.reverse_flg
        , calc.from_parts_name_diff_flg
        , calc.from_pin_no_diff_flg
        , calc.from_terminal_name_diff_flg
        , calc.from_parts_code_diff_flg
        , calc.to_parts_name_diff_flg
        , calc.to_pin_no_diff_flg
        , calc.to_terminal_name_diff_flg
        , calc.to_parts_code_diff_flg
        , calc.wire_color_diff_flg 
        
        , wiredetail.from_parts_code as detail_from_parts_code
        , wiredetail.from_parts_name as detail_from_parts_name 
        , wiredetail.from_wh_parts_name as detail_from_wh_parts_name
        , wiredetail.from_terminal_name as detail_from_terminal_name
        , wiredetail.from_pin_no as detail_from_pin_no
        , wiredetail.to_parts_code as detail_to_parts_code
        , wiredetail.to_parts_name as detail_to_parts_name
        , wiredetail.to_wh_parts_name as detail_to_wh_parts_name
        , wiredetail.to_terminal_name as detail_to_terminal_name
        , wiredetail.to_pin_no as detail_to_pin_no
        , wiredetail.wire_color as detail_wire_color
        
    from
        t_create_template tpl 
        inner join t_wire_list_pub_no wirepub 
            on ( 
                wirepub.wire_list_pub_no_id = tpl.wire_list_pub_no_id
            ) 
        inner join t_wire_list_detail wiredetail 
            on ( 
                wirepub.wire_list_pub_no_id = wiredetail.wire_list_pub_no_id
            ) 
        inner join t_similar_diagram_search diagramsearch 
            on ( 
                wirepub.wire_list_pub_no_id = diagramsearch.wire_list_pub_no_id
            ) 
        inner join t_create_template_image tplimage 
            on ( 
                diagramsearch.similar_diagram_search_id = tplimage.similar_circuit_search_id 
                and tplimage.create_template_id = tpl.create_template_id
				and tplimage.use_flg = 1
				and regexp_replace(wiring_file_name,'\.svg', '','i') = diagramsearch.fig_name
            ) 
        left outer join t_similar_calc calc 
            on ( 
                diagramsearch.similar_diagram_search_id = calc.similar_diagram_search_id
            ) 
    where
        tpl.create_template_id = #createTplId# 
        and calc.similar_point >= #min_point# 
    group by
        calc.wire_list_detail_id
        , calc.wire_id
        , calc.similar_point
        , calc.reverse_flg
        , calc.from_parts_name_diff_flg
        , calc.from_pin_no_diff_flg
        , calc.from_terminal_name_diff_flg
        , calc.from_parts_code_diff_flg
        , calc.to_parts_name_diff_flg
        , calc.to_pin_no_diff_flg
        , calc.to_terminal_name_diff_flg
        , calc.to_parts_code_diff_flg
        , calc.wire_color_diff_flg 
        
        , wiredetail.from_parts_code
        , wiredetail.from_parts_name 
        , wiredetail.from_wh_parts_name
        , wiredetail.from_terminal_name
        , wiredetail.from_pin_no
        , wiredetail.to_parts_code
        , wiredetail.to_parts_name
        , wiredetail.to_wh_parts_name
        , wiredetail.to_terminal_name
        , wiredetail.to_pin_no
        , wiredetail.wire_color
    order by
        calc.similar_point desc
)
--- 対象経線のFrom/To端子、色、SVGID情報の付与  
, wireids as ( 
    select
        calc.wire_list_detail_id
        , calc.wire_id
        , calc.similar_point
        , calc.reverse_flg
        , calc.from_parts_name_diff_flg
        , calc.from_pin_no_diff_flg
        , calc.from_terminal_name_diff_flg
        , calc.from_parts_code_diff_flg
        , calc.to_parts_name_diff_flg
        , calc.to_pin_no_diff_flg
        , calc.to_terminal_name_diff_flg
        , calc.to_parts_code_diff_flg
        , calc.wire_color_diff_flg
        , t_wire.from_terminal_id
        , t_wire.to_terminal_id
        , t_wire.svg_line_id
        , t_wire.wire_color 
        
        , calc.detail_from_parts_code
        , calc.detail_from_parts_name 
        , calc.detail_from_wh_parts_name
        , calc.detail_from_terminal_name
        , calc.detail_from_pin_no
        , calc.detail_to_parts_code
        , calc.detail_to_parts_name
        , calc.detail_to_wh_parts_name
        , calc.detail_to_terminal_name
        , calc.detail_to_pin_no
        , calc.detail_wire_color
    from
        calc_wireids calc 
        inner join t_wire 
            on (t_wire.wire_id = calc.wire_id)
) 

--- メモリ占有を圧縮するため、対象結線のPubNo、配膳図、パーツ＆端子情報のみを絞り込み 
, parts_terminal as ( 
    select
        pubno.pub_no
        , wirefig.fig_name
        , parts.wh_parts_name
        , parts.new_parts_cd
        , term.terminal_id
        , term.parts_id
        , term.terminal_name
        , term.pin_no
        , term.point_x
        , term.point_y
        , term.direction 
    from
        t_pub_no pubno 
        inner join t_wiring_diagram wirefig 
            on (pubno.pub_no_id = wirefig.pub_no_id) 
        inner join t_parts parts 
            on ( 
                parts.wiring_diagram_id = wirefig.wiring_diagram_id
            ) 
        inner join t_terminal term 
            on (parts.parts_id = term.parts_id) 
        inner join wireids 
            on ( 
                wireids.from_terminal_id = term.terminal_id 
                or wireids.to_terminal_id = term.terminal_id
            ) 
    group by
        pubno.pub_no
        , wirefig.fig_name
        , parts.wh_parts_name
        , parts.new_parts_cd
        , term.terminal_id
        , term.parts_id
        , term.terminal_name
        , term.pin_no
        , term.point_x
        , term.point_y
        , term.direction
)
--- 対象経線IDのFrom/To端子情報の付与 
    select wire.wire_id
        , wire.wire_list_detail_id
	    , fromterm.pub_no as pub_no
	    , fromterm.fig_name as fig_name
        , wire.similar_point
        , wire.reverse_flg
        , wire.from_parts_name_diff_flg
        , wire.from_pin_no_diff_flg
        , wire.from_terminal_name_diff_flg
        , wire.from_parts_code_diff_flg
        , wire.to_parts_name_diff_flg
        , wire.to_pin_no_diff_flg
        , wire.to_terminal_name_diff_flg
        , wire.to_parts_code_diff_flg
        , wire.wire_color_diff_flg
        , wire.from_terminal_id
        , wire.to_terminal_id
        , wire.svg_line_id
        , wire.wire_color
        
		, fromterm.wh_parts_name as from_wh_parts_name
        , fromterm.new_parts_cd as from_new_parts_cd
        , fromterm.parts_id as from_parts_id 
        , fromterm.terminal_name as from_terminal_name
        , fromterm.pin_no as from_pin_no
        , fromterm.point_x as from_point_x
        , fromterm.point_y as from_point_y
        , fromterm.direction as from_direction
		
        , toterm.wh_parts_name as to_wh_parts_name
        , toterm.new_parts_cd as to_new_parts_cd
        , toterm.parts_id as to_parts_id
        , toterm.terminal_name as to_terminal_name
        , toterm.pin_no as to_pin_no
        , toterm.point_x as to_point_x
        , toterm.point_y as to_point_y
        , toterm.direction as to_direction 
        
        , wire.detail_from_parts_code
        , wire.detail_from_parts_name 
        , wire.detail_from_wh_parts_name
        , wire.detail_from_terminal_name
        , wire.detail_from_pin_no
        , wire.detail_to_parts_code
        , wire.detail_to_parts_name
        , wire.detail_to_wh_parts_name
        , wire.detail_to_terminal_name
        , wire.detail_to_pin_no
        , wire.detail_wire_color
        
    from
        wireids wire 
        left outer join parts_terminal fromterm 
            on (wire.from_terminal_id = fromterm.terminal_id) 
        left outer join parts_terminal toterm 
            on (wire.to_terminal_id = toterm.terminal_id)
;
