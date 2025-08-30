from __future__ import annotations

import os
from datetime import datetime
from typing import Generator
from urllib.parse import quote_plus

from sqlalchemy import (
    Boolean,
    Column,
    DateTime,
    ForeignKey,
    Integer,
    String,
    Float,
    create_engine,
)
from sqlalchemy.orm import declarative_base, relationship, sessionmaker, Session


def _default_database_url() -> str:
    """Construct a default SQL Server URL if env variables are present."""
    server = os.getenv("MSSQL_SERVER")
    database = os.getenv("MSSQL_DB")
    user = os.getenv("MSSQL_USER")
    password = os.getenv("MSSQL_PASSWORD")
    if all([server, database, user, password]):
        pwd = quote_plus(password)
        trust_cert = os.getenv("MSSQL_TRUST_SERVER_CERTIFICATE", "no").lower() == "yes"
        conn_str = (
            f"mssql+pyodbc://{user}:{pwd}@{server}/{database}?"
            "driver=ODBC+Driver+18+for+SQL+Server&Encrypt=yes"
        )
        if trust_cert:
            conn_str += "&TrustServerCertificate=yes"
        return conn_str
    return "sqlite:///./dv8.db"


DATABASE_URL = os.getenv("DATABASE_URL", _default_database_url())

engine = create_engine(
    DATABASE_URL, connect_args={"check_same_thread": False} if DATABASE_URL.startswith("sqlite") else {}
)
SessionLocal = sessionmaker(bind=engine, autoflush=False, autocommit=False)
Base = declarative_base()


class Device(Base):
    __tablename__ = "devices"
    id = Column(Integer, primary_key=True)
    name = Column(String, unique=True, index=True, nullable=False)
    device_type = Column(String, default="unknown")
    ports = Column(Integer, default=0)
    status = Column(String, default="up")
    latitude = Column(Float, nullable=True)
    longitude = Column(Float, nullable=True)
    firmware = relationship("Firmware", uselist=False, back_populates="device")
    zero_touch = relationship("ZeroTouchConfig", uselist=False, back_populates="device")
    warnings = relationship("Warning", back_populates="device", cascade="all, delete-orphan")


class Firmware(Base):
    __tablename__ = "firmware"
    id = Column(Integer, primary_key=True)
    device_id = Column(Integer, ForeignKey("devices.id"), unique=True)
    version = Column(String, nullable=False)
    device = relationship("Device", back_populates="firmware")


class ZeroTouchConfig(Base):
    __tablename__ = "zero_touch"
    id = Column(Integer, primary_key=True)
    device_id = Column(Integer, ForeignKey("devices.id"), unique=True)
    template = Column(String, nullable=False)
    applied = Column(Boolean, default=False)
    device = relationship("Device", back_populates="zero_touch")


class Warning(Base):
    __tablename__ = "warnings"
    id = Column(Integer, primary_key=True)
    device_id = Column(Integer, ForeignKey("devices.id"))
    name = Column(String, nullable=False)
    active = Column(Boolean, default=True)
    device = relationship("Device", back_populates="warnings")


class AuditLog(Base):
    __tablename__ = "audit_log"
    id = Column(Integer, primary_key=True)
    action = Column(String, nullable=False)
    entity = Column(String, nullable=False)
    entity_id = Column(Integer, nullable=True)
    details = Column(String, nullable=True)
    timestamp = Column(DateTime, default=datetime.utcnow)


def init_db() -> None:
    Base.metadata.create_all(bind=engine)


def get_db() -> Generator[Session, None, None]:
    db = SessionLocal()
    try:
        yield db
    finally:
        db.close()
