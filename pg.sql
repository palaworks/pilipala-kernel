--
-- PostgreSQL database dump
--

-- Dumped from database version 14.2 (Ubuntu 14.2-1.pgdg21.10+1)
-- Dumped by pg_dump version 14.2 (Ubuntu 14.2-1.pgdg21.10+1)

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

--
-- Name: auth; Type: SCHEMA; Schema: -; Owner: postgres
--

CREATE SCHEMA auth;


ALTER SCHEMA auth OWNER TO postgres;

--
-- Name: container; Type: SCHEMA; Schema: -; Owner: postgres
--

CREATE SCHEMA container;


ALTER SCHEMA container OWNER TO postgres;

--
-- Name: tag; Type: SCHEMA; Schema: -; Owner: postgres
--

CREATE SCHEMA tag;


ALTER SCHEMA tag OWNER TO postgres;

SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- Name: token; Type: TABLE; Schema: auth; Owner: postgres
--

CREATE TABLE auth.token (
    "tokenHash" character varying(40) NOT NULL,
    ctime timestamp without time zone,
    atime timestamp without time zone
);


ALTER TABLE auth.token OWNER TO postgres;

--
-- Name: COLUMN token."tokenHash"; Type: COMMENT; Schema: auth; Owner: postgres
--

COMMENT ON COLUMN auth.token."tokenHash" IS '凭据哈希';


--
-- Name: COLUMN token.ctime; Type: COMMENT; Schema: auth; Owner: postgres
--

COMMENT ON COLUMN auth.token.ctime IS '创建时间';


--
-- Name: COLUMN token.atime; Type: COMMENT; Schema: auth; Owner: postgres
--

COMMENT ON COLUMN auth.token.atime IS '访问时间';


--
-- Name: comment; Type: TABLE; Schema: container; Owner: postgres
--

CREATE TABLE container.comment (
    "commentId" bigint NOT NULL,
    "ownerMetaId" bigint,
    "replyTo" bigint,
    nick character varying(32),
    content text,
    email character varying(64),
    site character varying(128),
    ctime timestamp without time zone
);


ALTER TABLE container.comment OWNER TO postgres;

--
-- Name: COLUMN comment."commentId"; Type: COMMENT; Schema: container; Owner: postgres
--

COMMENT ON COLUMN container.comment."commentId" IS '评论id';


--
-- Name: COLUMN comment."ownerMetaId"; Type: COMMENT; Schema: container; Owner: postgres
--

COMMENT ON COLUMN container.comment."ownerMetaId" IS '所属元信息id';


--
-- Name: COLUMN comment."replyTo"; Type: COMMENT; Schema: container; Owner: postgres
--

COMMENT ON COLUMN container.comment."replyTo" IS '回复到';


--
-- Name: COLUMN comment.nick; Type: COMMENT; Schema: container; Owner: postgres
--

COMMENT ON COLUMN container.comment.nick IS '昵称';


--
-- Name: COLUMN comment.content; Type: COMMENT; Schema: container; Owner: postgres
--

COMMENT ON COLUMN container.comment.content IS '内容';


--
-- Name: COLUMN comment.email; Type: COMMENT; Schema: container; Owner: postgres
--

COMMENT ON COLUMN container.comment.email IS '电子邮箱';


--
-- Name: COLUMN comment.site; Type: COMMENT; Schema: container; Owner: postgres
--

COMMENT ON COLUMN container.comment.site IS '站点';


--
-- Name: COLUMN comment.ctime; Type: COMMENT; Schema: container; Owner: postgres
--

COMMENT ON COLUMN container.comment.ctime IS '创建时间';


--
-- Name: meta; Type: TABLE; Schema: container; Owner: postgres
--

CREATE TABLE container.meta (
    "metaId" bigint NOT NULL,
    "superMetaId" bigint,
    "currRecordId" bigint,
    ctime timestamp without time zone,
    atime timestamp without time zone,
    view bigint,
    star bigint
);


ALTER TABLE container.meta OWNER TO postgres;

--
-- Name: COLUMN meta."metaId"; Type: COMMENT; Schema: container; Owner: postgres
--

COMMENT ON COLUMN container.meta."metaId" IS '元信息id';


--
-- Name: COLUMN meta."superMetaId"; Type: COMMENT; Schema: container; Owner: postgres
--

COMMENT ON COLUMN container.meta."superMetaId" IS '上级元信息id';


--
-- Name: COLUMN meta."currRecordId"; Type: COMMENT; Schema: container; Owner: postgres
--

COMMENT ON COLUMN container.meta."currRecordId" IS '当前记录id';


--
-- Name: COLUMN meta.ctime; Type: COMMENT; Schema: container; Owner: postgres
--

COMMENT ON COLUMN container.meta.ctime IS '创建时间';


--
-- Name: COLUMN meta.atime; Type: COMMENT; Schema: container; Owner: postgres
--

COMMENT ON COLUMN container.meta.atime IS '访问时间';


--
-- Name: COLUMN meta.view; Type: COMMENT; Schema: container; Owner: postgres
--

COMMENT ON COLUMN container.meta.view IS '浏览数';


--
-- Name: COLUMN meta.star; Type: COMMENT; Schema: container; Owner: postgres
--

COMMENT ON COLUMN container.meta.star IS '星星数';


--
-- Name: record; Type: TABLE; Schema: container; Owner: postgres
--

CREATE TABLE container.record (
    "recordId" bigint NOT NULL,
    cover text,
    title character varying(64),
    summary character varying(256),
    body text,
    mtime timestamp without time zone
);


ALTER TABLE container.record OWNER TO postgres;

--
-- Name: COLUMN record."recordId"; Type: COMMENT; Schema: container; Owner: postgres
--

COMMENT ON COLUMN container.record."recordId" IS '记录id';


--
-- Name: COLUMN record.cover; Type: COMMENT; Schema: container; Owner: postgres
--

COMMENT ON COLUMN container.record.cover IS '封面';


--
-- Name: COLUMN record.title; Type: COMMENT; Schema: container; Owner: postgres
--

COMMENT ON COLUMN container.record.title IS '标题';


--
-- Name: COLUMN record.summary; Type: COMMENT; Schema: container; Owner: postgres
--

COMMENT ON COLUMN container.record.summary IS '概述';


--
-- Name: COLUMN record.body; Type: COMMENT; Schema: container; Owner: postgres
--

COMMENT ON COLUMN container.record.body IS '正文';


--
-- Name: COLUMN record.mtime; Type: COMMENT; Schema: container; Owner: postgres
--

COMMENT ON COLUMN container.record.mtime IS '修改时间';


--
-- Name: invisible; Type: TABLE; Schema: tag; Owner: postgres
--

CREATE TABLE tag.invisible (
    "metaId" bigint
);


ALTER TABLE tag.invisible OWNER TO postgres;

--
-- Name: COLUMN invisible."metaId"; Type: COMMENT; Schema: tag; Owner: postgres
--

COMMENT ON COLUMN tag.invisible."metaId" IS '作用于元信息id';


--
-- Name: locked; Type: TABLE; Schema: tag; Owner: postgres
--

CREATE TABLE tag.locked (
    "metaId" bigint
);


ALTER TABLE tag.locked OWNER TO postgres;

--
-- Name: COLUMN locked."metaId"; Type: COMMENT; Schema: tag; Owner: postgres
--

COMMENT ON COLUMN tag.locked."metaId" IS '作用于元信息id';


--
-- Name: obsolete; Type: TABLE; Schema: tag; Owner: postgres
--

CREATE TABLE tag.obsolete (
    "metaId" bigint
);


ALTER TABLE tag.obsolete OWNER TO postgres;

--
-- Name: COLUMN obsolete."metaId"; Type: COMMENT; Schema: tag; Owner: postgres
--

COMMENT ON COLUMN tag.obsolete."metaId" IS '作用于元信息id';


--
-- Name: preview; Type: TABLE; Schema: tag; Owner: postgres
--

CREATE TABLE tag.preview (
    "metaId" bigint
);


ALTER TABLE tag.preview OWNER TO postgres;

--
-- Name: COLUMN preview."metaId"; Type: COMMENT; Schema: tag; Owner: postgres
--

COMMENT ON COLUMN tag.preview."metaId" IS '作用于元信息id';


--
-- Data for Name: token; Type: TABLE DATA; Schema: auth; Owner: postgres
--

COPY auth.token ("tokenHash", ctime, atime) FROM stdin;
1b06497b4203d224f51e24a1d6dd404b5df6d99c	2021-10-25 17:12:01	2021-10-25 17:12:01
de543181e3414576851698bc100017dbb6cb9851	2021-10-18 09:10:13	2021-11-03 10:16:31
\.


--
-- Data for Name: comment; Type: TABLE DATA; Schema: container; Owner: postgres
--

COPY container.comment ("commentId", "ownerMetaId", "replyTo", nick, content, email, site, ctime) FROM stdin;
1452268005785473024	0	123	黑手	小逼崽子	你是真没见过黑涩会哦	操你妈逼	2021-10-24 21:37:29
1452563559220383744	0	0					2021-10-25 17:11:55
\.


--
-- Data for Name: meta; Type: TABLE DATA; Schema: container; Owner: postgres
--

COPY container.meta ("metaId", "superMetaId", "currRecordId", ctime, atime, view, star) FROM stdin;
1452262572546985984	0	1452280065239945216	2021-10-24 21:15:54	2021-10-24 21:15:54	1	657687
1452266405020962816	1452275978192687104	1452280065239945216	2021-10-24 21:31:07	2021-10-24 21:31:07	0	0
1452275978192687104	1452262572546985984	0	2021-10-24 22:09:10	2021-10-24 22:09:10	0	0
1452450277608263680	0	0	2021-10-25 09:41:46	2021-10-25 09:41:46	0	0
1452563530443264000	0	0	2021-10-25 17:11:48	2021-10-25 17:11:48	0	0
1452991411313053696	0	1452991393248186368	2021-10-26 21:32:02	2021-10-26 21:32:02	0	0
1455101965506842624	0	0	2021-11-01 17:18:38	2021-11-01 17:18:38	0	0
1455704490635300864	0	1455704469873496064	2021-11-03 09:12:51	2021-11-03 09:12:51	0	0
\.


--
-- Data for Name: record; Type: TABLE DATA; Schema: container; Owner: postgres
--

COPY container.record ("recordId", cover, title, summary, body, mtime) FROM stdin;
1452266426122506240	ask	asgd	asgk	# palang 1.0.0	2021-10-24 21:31:12
1452280065239945216					2021-10-24 22:25:24
1452280141836324864					2021-10-24 22:25:43
1452441327475232768					2021-10-25 09:06:12
1452441344755765248					2021-10-25 09:06:16
1452450258415128576					2021-10-25 09:41:41
1452473539373436928					2021-10-25 11:14:12
1452563509366886400					2021-10-25 17:11:43
1452575631572340736					2021-10-25 17:59:53
1452804737769541632				1asg	2021-10-26 09:10:16
1452991393248186368					2021-10-26 21:31:58
1452994432885460992					2021-10-26 21:44:03
1453165699718582272				1234	2021-10-27 09:04:36
1453166086651514880					2021-10-27 09:06:08
1453166305191530496				____q\n	2021-10-27 09:07:00
1455018086850760704					2021-11-01 11:45:19
1455667259442860032					2021-11-03 06:44:54
1455703546413584384					2021-11-03 09:09:06
1455703599584776192					2021-11-03 09:09:18
1455703758347571200					2021-11-03 09:09:56
1455704469873496064					2021-11-03 09:12:46
\.


--
-- Data for Name: invisible; Type: TABLE DATA; Schema: tag; Owner: postgres
--

COPY tag.invisible ("metaId") FROM stdin;
1452262572546985984
1452266405020962816
\.


--
-- Data for Name: locked; Type: TABLE DATA; Schema: tag; Owner: postgres
--

COPY tag.locked ("metaId") FROM stdin;
\.


--
-- Data for Name: obsolete; Type: TABLE DATA; Schema: tag; Owner: postgres
--

COPY tag.obsolete ("metaId") FROM stdin;
\.


--
-- Data for Name: preview; Type: TABLE DATA; Schema: tag; Owner: postgres
--

COPY tag.preview ("metaId") FROM stdin;
\.


--
-- Name: token token_pk; Type: CONSTRAINT; Schema: auth; Owner: postgres
--

ALTER TABLE ONLY auth.token
    ADD CONSTRAINT token_pk PRIMARY KEY ("tokenHash");


--
-- Name: comment comment_pk; Type: CONSTRAINT; Schema: container; Owner: postgres
--

ALTER TABLE ONLY container.comment
    ADD CONSTRAINT comment_pk PRIMARY KEY ("commentId");


--
-- Name: meta meta_pk; Type: CONSTRAINT; Schema: container; Owner: postgres
--

ALTER TABLE ONLY container.meta
    ADD CONSTRAINT meta_pk PRIMARY KEY ("metaId");


--
-- Name: record record_pk; Type: CONSTRAINT; Schema: container; Owner: postgres
--

ALTER TABLE ONLY container.record
    ADD CONSTRAINT record_pk PRIMARY KEY ("recordId");


--
-- PostgreSQL database dump complete
--

