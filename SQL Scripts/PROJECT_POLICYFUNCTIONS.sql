
grant select on ADMIN.PROJECT_NHANSU to NVCOBAN;
create or replace function SEC_NVCOBAN_NHANSU_SELECT(P_SCHEMA VARCHAR2, P_OBJ VARCHAR2)
return varchar2
as
    cursor CUR_NHANVIEN is   
                                select MANV,VAITRO
                                from ADMIN.PROJECT_NHANSU
                                where MANV= sys_context('USERENV','SESSION_USER');

    manv varchar2(5);
    vaitro varchar2(40);
begin
    --vaitro := 'NVCOBAN';
    --manv := sys_context('USERENV','SESSION_USER');
    --open CUR_NHANVIEN;
    --fetch CUR_NHANVIEN into manv,vaitro;
    --if(vaitro = 'NVCOBAN') then
        --return 'MANV = ''' || manv ||'''';
    --else
        return '1=0';
    --end if;
end;

create or replace function SEC_DENY(P_SCHEMA VARCHAR2, P_OBJ VARCHAR2)
return varchar2
as
begin
        return '1=0';
end;

declare 
cursor CUR_NHANSU is    (
                                select MANV,VAITRO
                                from ADMIN.PROJECT_NHANSU
                            );
    ma varchar2(5);
    vt varchar2(40);
    nhansu ADMIN.PROJECT_NHANSU%ROWTYPE;
begin
    open CUR_NHANSU;
    --fetch CUR_NHANSU into ma,vt;
    dbms_output.put_line('good');

end;

