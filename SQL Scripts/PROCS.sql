create role NVCOBAN;
create role GIANGVIEN;
create role GIAOVU;
create role TRGKHOA;
create role TRGDONVI;

--BEGIN
--    dbms_rls.drop_policy(
--        object_schema     => 'ADMIN',
--        object_name      => 'PROJECT_NHANSU',
--        policy_name      => 'NVCOBAN_NHANSU_SELECT'
--    );
--END;
--
--BEGIN
--    dbms_rls.add_policy(
--        OBJECT_SCHEMA =>'ADMIN',
--        OBJECT_NAME=>'PROJECT_NHANSU',
--        POLICY_NAME =>'NVCOBAN_NHANSU_SELECT',
--        FUNCTION_SCHEMA => 'ADMIN',
--        POLICY_FUNCTION=>'SEC_NVCOBAN_NHANSU_SELECT',
--        SEC_RELEVANT_COLS => 'VAITRO'
--        );
--END;
--
--CREATE OR REPLACE PROCEDURE USP_CREATEUSER
--AS
--    CURSOR CUR IS (SELECT MAKH,VAITRO
--                    FROM ADMIN.PROJECT_NHANSU
--                    WHERE MAKH NOT IN (SELECT USERNAME
--                                        FROM ALL_USERS));
--    STRSQL VARCHAR(2000);
--    USR VARCHAR2(5);
--    VAITRO varchar2(40);
--BEGIN
--    OPEN CUR;
--    STRSQL := 'ALTER SESSION SET "_ORACLE_SCRIPT" = TRUE';
--    EXECUTE IMMEDIATE(STRSQL);
--    LOOP
--        FETCH CUR INTO USR,VAITRO;
--        EXIT WHEN CUR%NOTFOUND;
--            
--        STRSQL := 'CREATE USER '||USR||' IDENTIFIED BY '||USR;
--        EXECUTE IMMEDIATE(STRSQL);
--        STRSQL := 'GRANT CONNECT TO '||USR;
--        EXECUTE IMMEDIATE(STRSQL);
--        STRSQL := 'GRANT ' || VAITRO || 'TO ' || USR;
--        EXECUTE IMMEDIATE(STRSQL);
--    END LOOP;
--    STRSQL := 'ALTER SESSION SET "_ORACLE_SCRIPT" = FALSE';
--    EXECUTE IMMEDIATE(STRSQL);
--    CLOSE CUR;
--END;


--SELECT * FROM DBA_ROLES;

--ALTER SESSION SET "_ORACLE_SCRIPT" = TRUE;
--create user NV001 identified by NV001;
--grant connect to NV001;
--grant NVCOBAN to NV001;

create or replace procedure grant_privilege(
    operation NVARCHAR2,
    owner NVARCHAR2,
    tableName NVARCHAR2,
    schemaName NVARCHAR2,
    columnList NVARCHAR2,
    grantOption NVARCHAR2
)as
    STRSQL NVARCHAR2(2000);
BEGIN
    if (operation = 'SELECT' and columnList != ' ') then
        BEGIN
            STRSQL := 'create or replace view ' || owner || '.' ||  schemaName || '_' || tableName || '_' || operation || 
                        '(' || columnList || ') as select ' || columnList || ' from ' || owner || '.' || tableName;
            dbms_output.put_line(STRSQL);
            EXECUTE IMMEDIATE(STRSQL);
            STRSQL := 'GRANT ' || operation || ' on ' || owner || '.' || schemaName || '_' || tableName || '_' || operation || ' to ' || schemaName;
            if(grantOption != ' ') then
                STRSQL := STRSQL || ' ' || grantOption;
            end if;
            dbms_output.put_line(STRSQL);
            EXECUTE IMMEDIATE(STRSQL);

        END;
    elsif (operation = 'UPDATE' and columnList != ' ') then
        BEGIN
            STRSQL := 'GRANT ' || operation || ' (' || columnList || ') ' || ' on ' || owner || '.' || tableName || ' to ' || schemaName;
            if(grantOption != ' ') then
                STRSQL := STRSQL || ' ' || grantOption;
            end if;
            dbms_output.put_line(STRSQL);
            EXECUTE IMMEDIATE(STRSQL);
        END;
    else
        BEGIN
            STRSQL := 'GRANT ' || operation || ' on ' || owner || '.' || tableName || ' to ' || schemaName;
            if(grantOption != ' ') then
                STRSQL := STRSQL || ' ' || grantOption;
            end if;
            dbms_output.put_line(STRSQL);
            EXECUTE IMMEDIATE(STRSQL);
        END;
    end if;
END;

--execute grant_privilege('SELECT','ADMIN','PROJECT_DONVI','NV001',' ', 'WITH GRANT OPTION');
--revoke select on ADMIN.PROJECT_DONVI from NV001;

create or replace procedure grant_role(
    username nvarchar2,
    rolename nvarchar2,
    adminOption nvarchar2
)as
    STRSQL nvarchar2(1000);
begin
    STRSQL := 'GRANT ' || rolename || ' to ' || username || ' ' || adminOption;
    dbms_output.put_line(STRSQL);
    EXECUTE IMMEDIATE(STRSQL);
end;

--execute grant_role('NV001', 'MANAGEMENT','');

create or replace procedure revoke_privilege(
    username nvarchar2,
    operation nvarchar2,
    tableOwner nvarchar2,
    tableName nvarchar2
)as
    STRSQL nvarchar2(1000);
BEGIN
    STRSQL := 'ALTER SESSION SET "_ORACLE_SCRIPT" = TRUE';
    EXECUTE IMMEDIATE(STRSQL);
    STRSQL := 'REVOKE ' || operation || ' on ' || tableOwner || '.' || tableName || ' from ' || username;
    dbms_output.put_line(STRSQL);
    EXECUTE IMMEDIATE(STRSQL);
END;

--execute revoke_privilege('DATAENTRY','UPDATE','SYS','BTTH_KHACHHANG');

--ALTER SESSION SET "_ORACLE_SCRIPT" = TRUE;
--revoke UPDATE on SYS.ACCESS$ from DATAENTRY;

create or replace procedure revoke_role(
    username nvarchar2,
    rolename nvarchar2
)as
    STRSQL nvarchar2(1000);
BEGIN
    STRSQL := 'ALTER SESSION SET "_ORACLE_SCRIPT" = TRUE';
    EXECUTE IMMEDIATE(STRSQL);
    STRSQL := 'REVOKE ' || rolename || ' from ' || username;
    dbms_output.put_line(STRSQL);
    EXECUTE IMMEDIATE(STRSQL);
END;

execute revoke_role('DG001','DATAENTRY')

create or replace procedure delete_user(
    username nvarchar2
)as
    STRSQL nvarchar2(1000);
BEGIN
    STRSQL := 'DROP USER ' || username;
    dbms_output.put_line(STRSQL);
    EXECUTE IMMEDIATE(STRSQL);
END;


--execute delete_user('NV002');

create or replace procedure delete_role(
    rolename nvarchar2
)as
    STRSQL nvarchar2(1000);
BEGIN
    STRSQL := 'DROP ROLE ' || rolename;
    dbms_output.put_line(STRSQL);
    EXECUTE IMMEDIATE(STRSQL);
END;

create or replace procedure create_user(
    username nvarchar2,
    password nvarchar2
)as
    STRSQL nvarchar2(1000);
BEGIN
    STRSQL := 'ALTER SESSION SET "_ORACLE_SCRIPT" = TRUE';
    EXECUTE IMMEDIATE(STRSQL);
    STRSQL := 'CREATE USER ' || username || ' identified by "' || password || '"';
    dbms_output.put_line(STRSQL);
    EXECUTE IMMEDIATE(STRSQL);
END;

create or replace procedure create_role(
    name nvarchar2
)as
    STRSQL nvarchar2(1000);
BEGIN
    STRSQL := 'ALTER SESSION SET "_ORACLE_SCRIPT" = TRUE';
    EXECUTE IMMEDIATE(STRSQL);
    STRSQL := 'CREATE ROLE ' || name;
    dbms_output.put_line(STRSQL);
    EXECUTE IMMEDIATE(STRSQL);
END;

create or replace procedure change_password(
    username nvarchar2,
    password nvarchar2
)as
    STRSQL nvarchar2(1000);
BEGIN
    STRSQL := 'ALTER SESSION SET "_ORACLE_SCRIPT" = TRUE';
    EXECUTE IMMEDIATE(STRSQL);
    STRSQL := 'ALTER USER ' || username || ' identified by "' || password || '"';
    dbms_output.put_line(STRSQL);
    EXECUTE IMMEDIATE(STRSQL);
END;

--select * from dba_users

