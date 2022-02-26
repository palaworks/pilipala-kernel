/*
 Navicat Premium Data Transfer

 Source Server         : localhost
 Source Server Type    : MySQL
 Source Server Version : 80022
 Source Host           : localhost:3306
 Source Schema         : pilipala_fs

 Target Server Type    : MySQL
 Target Server Version : 80022
 File Encoding         : 65001

 Date: 26/02/2022 08:17:30
*/

SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

-- ----------------------------
-- Table structure for comment
-- ----------------------------
DROP TABLE IF EXISTS `comment`;
CREATE TABLE `comment`  (
  `commentId` bigint UNSIGNED NOT NULL DEFAULT 0 COMMENT '评论id',
  `ownerStackId` bigint UNSIGNED NULL DEFAULT 0 COMMENT '所属栈id',
  `replyTo` bigint UNSIGNED NULL DEFAULT 0 COMMENT '回复到',
  `nick` varchar(32) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT '' COMMENT '昵称',
  `content` text CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL COMMENT '内容',
  `email` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT '' COMMENT '电子邮箱',
  `site` varchar(128) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT '' COMMENT '站点',
  `ctime` datetime NULL DEFAULT NULL COMMENT '创建时间',
  PRIMARY KEY (`commentId`) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of comment
-- ----------------------------
INSERT INTO `comment` VALUES (1452268005785473024, 0, 123, '黑手', '小逼崽子', '你是真没见过黑涩会哦', '操你妈逼', '2021-10-24 21:37:29');
INSERT INTO `comment` VALUES (1452563559220383744, 0, 0, '', '', '', '', '2021-10-25 17:11:55');

-- ----------------------------
-- Table structure for record
-- ----------------------------
DROP TABLE IF EXISTS `record`;
CREATE TABLE `record`  (
  `recordId` bigint NOT NULL DEFAULT 0 COMMENT '记录id',
  `cover` text CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL COMMENT '封面',
  `title` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT '' COMMENT '标题',
  `summary` varchar(256) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT '' COMMENT '概述',
  `body` mediumtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL COMMENT '正文',
  `mtime` datetime NULL DEFAULT NULL COMMENT '修改时间',
  PRIMARY KEY (`recordId`) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of record
-- ----------------------------
INSERT INTO `record` VALUES (1452266426122506240, 'ask', 'asgd', 'asgk', '# palang 1.0.0', '2021-10-24 21:31:12');
INSERT INTO `record` VALUES (1452280065239945216, '', '', '', '', '2021-10-24 22:25:24');
INSERT INTO `record` VALUES (1452280141836324864, '', '', '', '', '2021-10-24 22:25:43');
INSERT INTO `record` VALUES (1452441327475232768, '', '', '', '', '2021-10-25 09:06:12');
INSERT INTO `record` VALUES (1452441344755765248, '', '', '', '', '2021-10-25 09:06:16');
INSERT INTO `record` VALUES (1452450258415128576, '', '', '', '', '2021-10-25 09:41:41');
INSERT INTO `record` VALUES (1452473539373436928, '', '', '', '', '2021-10-25 11:14:12');
INSERT INTO `record` VALUES (1452563509366886400, '', '', '', '', '2021-10-25 17:11:43');
INSERT INTO `record` VALUES (1452575631572340736, '', '', '', '', '2021-10-25 17:59:53');
INSERT INTO `record` VALUES (1452804737769541632, '', '', '', '1asg', '2021-10-26 09:10:16');
INSERT INTO `record` VALUES (1452991393248186368, '', '', '', '', '2021-10-26 21:31:58');
INSERT INTO `record` VALUES (1452994432885460992, '', '', '', '', '2021-10-26 21:44:03');
INSERT INTO `record` VALUES (1453165699718582272, '', '', '', '1234', '2021-10-27 09:04:36');
INSERT INTO `record` VALUES (1453166086651514880, '', '', '', '', '2021-10-27 09:06:08');
INSERT INTO `record` VALUES (1453166305191530496, '', '', '', '____q\n', '2021-10-27 09:07:00');
INSERT INTO `record` VALUES (1455018086850760704, '', '', '', '', '2021-11-01 11:45:19');
INSERT INTO `record` VALUES (1455667259442860032, '', '', '', '', '2021-11-03 06:44:54');
INSERT INTO `record` VALUES (1455703546413584384, '', '', '', '', '2021-11-03 09:09:06');
INSERT INTO `record` VALUES (1455703599584776192, '', '', '', '', '2021-11-03 09:09:18');
INSERT INTO `record` VALUES (1455703758347571200, '', '', '', '', '2021-11-03 09:09:56');
INSERT INTO `record` VALUES (1455704469873496064, '', '', '', '', '2021-11-03 09:12:46');

-- ----------------------------
-- Table structure for stack
-- ----------------------------
DROP TABLE IF EXISTS `stack`;
CREATE TABLE `stack`  (
  `stackId` bigint UNSIGNED NOT NULL DEFAULT 0 COMMENT '栈id',
  `superStackId` bigint UNSIGNED NULL DEFAULT 0 COMMENT '上级栈id',
  `currRecordId` bigint UNSIGNED NULL DEFAULT 0 COMMENT '当前记录id',
  `ctime` datetime NULL DEFAULT NULL COMMENT '创建时间',
  `atime` datetime NULL DEFAULT NULL COMMENT '访问时间',
  `view` int UNSIGNED NULL DEFAULT 0 COMMENT '浏览数',
  `star` int UNSIGNED NULL DEFAULT 0 COMMENT '星星数',
  PRIMARY KEY (`stackId`) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of stack
-- ----------------------------
INSERT INTO `stack` VALUES (1452262572546985984, 0, 1452280065239945216, '2021-10-24 21:15:54', '2021-10-24 21:15:54', 1, 657687);
INSERT INTO `stack` VALUES (1452266405020962816, 1452275978192687104, 1452280065239945216, '2021-10-24 21:31:07', '2021-10-24 21:31:07', 0, 0);
INSERT INTO `stack` VALUES (1452275978192687104, 1452262572546985984, 0, '2021-10-24 22:09:10', '2021-10-24 22:09:10', 0, 0);
INSERT INTO `stack` VALUES (1452450277608263680, 0, 0, '2021-10-25 09:41:46', '2021-10-25 09:41:46', 0, 0);
INSERT INTO `stack` VALUES (1452563530443264000, 0, 0, '2021-10-25 17:11:48', '2021-10-25 17:11:48', 0, 0);
INSERT INTO `stack` VALUES (1452991411313053696, 0, 1452991393248186368, '2021-10-26 21:32:02', '2021-10-26 21:32:02', 0, 0);
INSERT INTO `stack` VALUES (1455101965506842624, 0, 0, '2021-11-01 17:18:38', '2021-11-01 17:18:38', 0, 0);
INSERT INTO `stack` VALUES (1455704490635300864, 0, 1455704469873496064, '2021-11-03 09:12:51', '2021-11-03 09:12:51', 0, 0);

-- ----------------------------
-- Table structure for tag_invisible
-- ----------------------------
DROP TABLE IF EXISTS `tag_invisible`;
CREATE TABLE `tag_invisible`  (
  `stackId` bigint NOT NULL,
  PRIMARY KEY (`stackId`) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of tag_invisible
-- ----------------------------
INSERT INTO `tag_invisible` VALUES (1452262572546985984);
INSERT INTO `tag_invisible` VALUES (1452266405020962816);

-- ----------------------------
-- Table structure for tag_locked
-- ----------------------------
DROP TABLE IF EXISTS `tag_locked`;
CREATE TABLE `tag_locked`  (
  `stackId` bigint NOT NULL,
  PRIMARY KEY (`stackId`) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of tag_locked
-- ----------------------------

-- ----------------------------
-- Table structure for tag_obsolete
-- ----------------------------
DROP TABLE IF EXISTS `tag_obsolete`;
CREATE TABLE `tag_obsolete`  (
  `stackId` bigint NOT NULL,
  PRIMARY KEY (`stackId`) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of tag_obsolete
-- ----------------------------

-- ----------------------------
-- Table structure for tag_preview
-- ----------------------------
DROP TABLE IF EXISTS `tag_preview`;
CREATE TABLE `tag_preview`  (
  `stackId` bigint NOT NULL,
  PRIMARY KEY (`stackId`) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of tag_preview
-- ----------------------------

-- ----------------------------
-- Table structure for token
-- ----------------------------
DROP TABLE IF EXISTS `token`;
CREATE TABLE `token`  (
  `tokenHash` varchar(40) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL DEFAULT '' COMMENT '凭据哈希',
  `ctime` datetime NULL DEFAULT NULL COMMENT '创建时间',
  `atime` datetime NULL DEFAULT NULL COMMENT '访问时间',
  PRIMARY KEY (`tokenHash`) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of token
-- ----------------------------
INSERT INTO `token` VALUES ('1b06497b4203d224f51e24a1d6dd404b5df6d99c', '2021-10-25 17:12:01', '2021-10-25 17:12:01');
INSERT INTO `token` VALUES ('de543181e3414576851698bc100017dbb6cb9851', '2021-10-18 09:10:13', '2021-11-03 10:16:31');

SET FOREIGN_KEY_CHECKS = 1;
