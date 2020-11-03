# Host: localhost  (Version 5.7.11)
# Date: 2019-11-28 00:17:13
# Generator: MySQL-Front 6.1  (Build 1.26)


#
# Structure for table "counter"
#

CREATE TABLE `counter` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `PassWord` varchar(16) NOT NULL DEFAULT '12345678',
  `name` varchar(255) NOT NULL DEFAULT '新玩家',
  `markName` varchar(255) DEFAULT NULL COMMENT '好友备注',
  `mileage` int(11) DEFAULT '0' COMMENT '要删除',
  `icon` varchar(255) NOT NULL DEFAULT 'default' COMMENT '头像',
  `sex` tinyint(2) NOT NULL DEFAULT '0' COMMENT '0未知；1男；2女',
  `age` tinyint(2) NOT NULL DEFAULT '0',
  `winRate` float(5,4) NOT NULL DEFAULT '0.5000' COMMENT '胜率',
  `serialWin` mediumint(8) NOT NULL DEFAULT '0' COMMENT '最高连胜场数',
  `gameNum` mediumint(8) NOT NULL DEFAULT '0' COMMENT '总对局数',
  `degree` tinyint(3) NOT NULL DEFAULT '0' COMMENT '等级（500）段位（10个段）',
  `status` tinyint(2) NOT NULL DEFAULT '0' COMMENT '状态（5个）0：离线 ；1：在线；2：组队；3：准备；4：游戏中；',
  `roomNum` varchar(255) NOT NULL DEFAULT '0' COMMENT '房间号',
  `coin` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`Id`)
) ENGINE=MyISAM AUTO_INCREMENT=307163611 DEFAULT CHARSET=utf8 COMMENT='账号密码';

#
# Data for table "counter"
#

INSERT INTO `counter` VALUES (1,'1','还汇1',NULL,11,'c1',1,2,0.5000,0,0,0,3,'0',0),(2,'2','玩家2',NULL,0,'c2',0,21,0.0400,0,0,0,3,'0',0),(11,'11','r',NULL,0,'c4',0,48,1.0000,0,0,0,0,'0',0),(12,'12','12科技德',NULL,11,'c3',1,2,0.3000,0,0,0,0,'0',0),(22,'22','新玩家',NULL,0,'c2',0,0,0.5000,0,0,0,0,'0',0),(36,'44','yuutu',NULL,14,'c2',0,69,0.6000,11,23,21,0,'0',0),(45,'1','新玩家66',NULL,0,'default',0,0,0.5000,0,0,0,0,'0',0),(56,'656','yyy',NULL,45,'c3',0,11,0.2000,0,0,0,0,'0',0),(66,'12345678','新玩家',NULL,0,'default',0,0,0.5000,0,0,0,0,'0',0);

#
# Structure for table "friends"
#

CREATE TABLE `friends` (
  `User_id` int(11) NOT NULL DEFAULT '0',
  `F_id` int(8) NOT NULL DEFAULT '0',
  PRIMARY KEY (`User_id`,`F_id`)
) ENGINE=MyISAM AUTO_INCREMENT=13 DEFAULT CHARSET=utf8;

#
# Data for table "friends"
#

INSERT INTO `friends` VALUES (1,2),(1,11),(1,12),(1,22),(1,36),(1,66),(2,1),(2,11),(2,12),(2,22),(2,66),(11,1),(11,2),(11,12),(12,1),(12,2),(12,11),(12,22),(12,56),(12,66),(56,1),(56,11),(56,12),(56,22);
